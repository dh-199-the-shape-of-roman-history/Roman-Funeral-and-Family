// Extend javascript array class by a remove function.
// Copied from https://stackoverflow.com/a/3955096/12267732
Array.prototype.remove = function() {
    let what,
        a = arguments,
        L = a.length,
        ax;
    while (L && this.length) {
        what = a[--L];
        while ((ax = this.indexOf(what)) !== -1) {
            this.splice(ax, 1);
        }
    }
    return this;
};

/**
 * Given a year, where BCE is represented by negative numbers,
 * output it as a string.
 *
 * @param int year The year as an int.
 * @return string The year as a string with BCE or CE.
 */
function print_year(year) {
    // Special cases for bad input.
    if (typeof year === 'undefined') {
        return '?';
    }
    if (typeof year != 'number') {
        return year;
    }
    if (year >= 0) {
        return year + ' CE';
    }
    return (-1 * year) + ' BCE';
}


/**
 * Class that contains all nodes and edges, handles setup of the tree, and allows for node lookup.
 *
 * NOTE: Current expansion logic requires all nodes to be connected.  Matrix calculation errors can occur
 * otherwise.
 *
 * Each object has a reference to the data handler, allowing nodes to find their parents.
 */
class FTDataHandler {

    /**
     * Builds the graph from the provided data.
     *
     * @param mixed data JSON data for the family tree, as specified in the readme.
     * @param mixed start_node_id Optional. The ID of the start node. Default data.start.
     */
    constructor(data, start_node_id = data.start) {
        // Check if edge list is defined.
        if (data.links.length > 0) {
            // Make DAG from edge list.
            this.dag = d3.dagConnect()(data.links);

            // dag must be a node with id undefined. Fix if necessary.
            if (this.dag.id != undefined) {
                this.root = this.dag.copy();
                this.root.id = undefined;
                this.root.children = [this.dag];
                this.dag = this.root;
            }

            // Get all d3-dag nodes and convert to family tree nodes.
            // Person and Union (marriage) are the two node types used.
            this.nodes = this.dag.descendants().map(node => {
                if (node.id in data.unions) {
                    return new Union(node, this);
                }
                else if (node.id in data.persons) {
                    return new Person(node, this);
                }
            });

            // Relink children arrays: use family tree nodes instead of d3-dag nodes.
            this.nodes.forEach(n => n._children = n._children.map(c => c.ftnode));

            // Make sure each node has an id.
            // NOTE: If pre-set ids are integers, this may break.
            this.number_nodes = 0;
            this.nodes.forEach(node => {
                node.id = node.id || this.number_nodes;
                this.number_nodes++;
            })

            // Set root node. The graph will center around this.
            this.root = this.find_node_by_id(start_node_id);
            this.root.visible = true;
            this.root.is_start = true;
            this.dag.children = [this.root];
        } else if (Object.values(data.persons).length > 0) {
            // If no edges are present and only nodes are defined: root = dag.
            const root_data = data.persons[start_node_id];
            this.root = new d3.dagNode(start_node_id, root_data);
            // Convert the root to a Person.
            this.root = new Person(this.root, this);
            this.root.visible = true;
            this.root.is_start = true;
            this.number_nodes = 1;
            this.nodes = [this.root];

            // dag must be a node with id undefined.
            this.dag = new d3.dagNode(undefined, {});
            this.dag.children = this.root;
        }
    };

    /**
     * After the graph is built, give each genealogical child knowledge of the Union containing its parents.
     * Then, start the recursive expansion of the graph.
     */
    setup_parents() {
        const new_nodes = [];
        // Look up the Union that serves as the parent to each Person, if it exists.
        // This is slow now but allows for instant lookup later.
        this.nodes.forEach(node => {
            if (node.constructor.name == 'Union') {
                if (node.data.children.length == 0) return;
                node.data.children.forEach(childId => {
                    this.nodes[this.nodes.findIndex(node => node.id == childId)].data.parent_union = node.id;
                });
            }
        });
        // The graph starts with only the starting node visible.  Recursively expand all connected nodes.
        this.root.expand();
    }

    /**
     * Update the children nodes marked in dag by recursively searching for roots.
     *
     * Note: This is required by the underlying DAG functionality.  Calling this after each expansion of the graph
     * improves the appearance of simple graphs and prevents crashes in complex graphs.
     * Do not simply set this.dag.children to this.nodes.
     */
    update_roots() {
        this.dag.children = [this.root];
        const FT = this;

        // Note: In overly large graphs, this may cause a stack overflow error.
        function find_roots_recursive(node) {
            node.get_visible_inserted_neighbors().forEach(node => {
                if (node.is_root()) {
                    FT.dag.children.push(node);
                }
                find_roots_recursive(node);
            });
        };
        find_roots_recursive(this.root);
    };

    /**
     * Given a node ID, returns the corresponding node.
     *
     * @param mixed id The ID of the node to find.
     * @return FTNode The node with this id. Returns undefined if no matches are found.
     */
    find_node_by_id(id) {
        return this.nodes.find(node => node.id == id);
    };
};

/**
 * Class that represents any node in a Family Tree graph.
 *
 * Adds functionality to help update the DAG's roots based on which nodes have been added (are visible).
 */
class FTNode extends d3.dagNode {
    get_visible_neighbors() {
        return this.get_neighbors().filter(node => node.visible);
    }

    get_visible_inserted_neighbors() {
        return this.get_visible_neighbors().filter(node => this.inserted_nodes.includes(node));
    };
};

/**
 * Class that represents a marriage node in the family tree.
 *
 * Has two parents and 0 or more children.
 */
class Union extends FTNode {
    /**
     * Builds a Union out of a standard d3.dagNode.
     */
    constructor(dagNode, ft_datahandler) {
        super(dagNode.id, data.unions[dagNode.id]);
        // Link the dagNode to this new object.
        // NOTE: There are likely cleaner ways to do this.
        dagNode.ftnode = this;
        // Define additional family tree properties.
        this.ft_datahandler = ft_datahandler;
        // Represents the dagNode children.
        this._children = dagNode.children;
        // Represents the FTNode children.
        this.children = [];
        this._childLinkData = dagNode._childLinkData;
        this.inserted_nodes = [];
        this.visible = false;
        this.expanded = false;
        this.is_start = false;
    };

    /**
     * Recursively expands this union for display in the family tree by
     * expanding children (if any) and both parents.
     */
    expand() {
        // Prevent infinite recursion by only allowing expansion of a given Union once.
        if (this.expanded) {
            return;
        }
        this.expanded = true;
        // Each union is responsible for setting itself, its parents, and its children to visible.
        // This also updates necessary fields connecting everyone together.
        this.show();
        this.ft_datahandler.update_roots();
        // Recursively expand children.
        this.children.forEach(child => child.expand());
        // Recursively expand parents.
        this.get_parents().forEach(parent => parent.expand());
    }

    get_neighbors() {
        return this.get_parents().concat(this.get_children())
    };

    get_parents() {
        let parents = this.data.partner
            .map(id => this.ft_datahandler.find_node_by_id(id))
            .filter(node => node != undefined);
        if (parents) {
            return parents;
        } else {
            return [];
        }
    }

    get_hidden_parents() {
        return this.get_parents().filter(parent => !parent.visible);
    };

    get_visible_parents() {
        return this.get_parents().filter(parent => parent.visible);
    };

    get_children() {
        let children = [];
        children = this.children.concat(this._children);
        // Filter out undefined children.
        children = children
            .filter(c => c != undefined)
        return children
    };

    get_hidden_children() {
        return this.get_children().filter(child => !child.visible);
    };

    get_visible_children() {
        return this.get_children().filter(child => child.visible);
    };

    show_child(child) {
        if (!this._children.includes(child)) {
            console.warn("Child node not in this' _children array.");
        }
        this.children.push(child);
        this._children.remove(child);
        // If child is hidden, show it.
        if (!child.visible) {
            child.visible = true;
            this.inserted_nodes.push(child);
        }
    };

    show_parent(parent) {
        if (!parent._children.includes(this)) {
            console.warn("This node not in parent's _children array.");
        }
        parent.children.push(this);
        parent._children.remove(this);
        // If parent is hidden, show it.
        if (!parent.visible) {
            parent.visible = true;
            this.inserted_nodes.push(parent);
        }
    };

    show() {
        this.visible = true;
        // Fill in the children field for neighboring children and parents.
        this.get_children().forEach(child => {
            this.show_child(child);
        });
        this.get_parents().forEach(parent => {
            this.show_parent(parent);
        });

    };

    get_visible_inserted_children() {
        return this.children.filter(child => this.inserted_nodes.includes(child));
    };

    get_visible_inserted_parents() {
        return this.get_visible_parents().filter(parent => this.inserted_nodes.includes(parent));
    };

    is_root() {
        return false;
    }

    get_own_unions() {
        return [];
    };

    get_parent_unions() {
        return [];
    };

    get_name() {
        return undefined;
    };

    get_birth_year() {
        return undefined;
    };

    get_death_year() {
        return undefined;
    };

    is_union() {
        return true;
    };
};

/**
 * Class that represents a person node in the family tree.
 *
 * A person is able to find its own Union(s) and the Union where it is a child.
 */
class Person extends FTNode {
    constructor(dagNode, ft_datahandler) {
        super(dagNode.id, data.persons[dagNode.id]);
        // Link the d3.dagNode to this new object.
        dagNode.ftnode = this;
        // Define additional family tree properties.
        this.ft_datahandler = ft_datahandler;
        this._children = dagNode.children;
        this.children = [];
        this._childLinkData = dagNode._childLinkData;
        this.inserted_nodes = [];
        this.visible = false;
        this.expanded = false;
        this.is_start = false;
    };

    get_name() {
        return this.data.name;
    };

    get_birth_year() {
        return this.data.birthyear;
    };

    get_death_year() {
        return this.data.deathyear;
    };

    get_neighbors() {
        return this.get_own_unions().concat(this.get_parent_unions());
    };

    get_parent_unions() {
        let unions = [this.data.parent_union]
            .map(id => this.ft_datahandler.find_node_by_id(id))
            .filter(node => node != undefined);
        if (unions) {
            return unions;
        } else {
            return [];
        }
    };

    get_hidden_parent_unions() {
        return this.get_parent_unions().filter(union => !union.visible);
    };

    get_visible_parent_unions() {
        return this.get_parent_unions().filter(union => union.visible);
    };

    get_visible_inserted_parent_unions() {
        return this.get_visible_parent_unions().filter(union => this.inserted_nodes.includes(union));
    };

    is_root() {
        return this.get_visible_parent_unions().length == 0;
    };

    is_union() {
        return false;
    };

    get_own_unions() {
        let unions = (this.data.own_unions ?? [])
            .map(id => this.ft_datahandler.find_node_by_id(id))
            .filter(u => u != undefined);
        if (unions) {
            return unions;
        } else {
            return [];
        }
    };

    get_hidden_own_unions() {
        return this.get_own_unions().filter(union => !union.visible);
    };

    get_visible_own_unions() {
        return this.get_own_unions().filter(union => union.visible);
    };

    get_visible_inserted_own_unions() {
        return this.get_visible_own_unions().filter(union => this.inserted_nodes.includes(union));
    };

    get_parents() {
        let parents = [];
        this.get_parent_unions().forEach(
            u => parents = parents.concat(u.get_parents())
        );
        return parents;
    };

    get_other_partner(union_data) {
        let partner_id = union_data.partner.find(
            p_id => p_id != this.id & p_id != undefined
        );
        return all_nodes.find(n => n.id == partner_id);
    };

    get_partners() {
        let partners = [];
        this.get_own_unions().forEach(
            u => {
                partners.push(this.get_other_partner(u.data))
            }
        )
        return partners.filter(p => p != undefined);
    };

    get_children() {
        let children = [];
        this.get_own_unions().forEach(
                u => children = children.concat(getChildren(u))
            )
            // Filter out undefined children.
        children = children
            .filter(c => c != undefined)
        return children;
    };

    add_inserted_nodes() {
        this.get_hidden_own_unions().forEach(union => this.inserted_nodes.push(union));
        this.get_hidden_parent_unions().forEach(union => this.inserted_nodes.push(union));
    };

    /**
     * Expands this person by showing their spouses, children, parents, and siblings recursively.
     */
    expand() {
        // Avoid infinite recursion.
        if (this.expanded) {
            return;
        }
        this.expanded = true;
        this.add_inserted_nodes();
        // Recursively expand children and spouse.
        this.get_hidden_own_unions().forEach(union => union.expand());
        this.ft_datahandler.update_roots();
        // Recursively expand siblings and parents.
        this.get_hidden_parent_unions().forEach(union => union.expand());
        this.ft_datahandler.update_roots();
    };
};

/**
 * Class that handles rendering of the Family Tree, specifying instructions for
 * tooltips, Unions, Persons, and Links.
 */
class FTDrawer {
    static label_delimiter = "_";

    constructor(
        ft_datahandler,
        svg,
        x0,
        y0,
    ) {
        this.ft_datahandler = ft_datahandler;
        this.svg = svg;
        this._orientation = null;
        this.link_css_class = "link";

        // Append group element to draw family tree in.
        this.g = this.svg.append("g");

        // Initialize panning, zooming.
        this.zoom = d3.zoom().on("zoom", event => this.g.attr("transform", event.transform));
        this.svg.call(this.zoom);

        // Initialize tooltips.
        this._tooltip_div = d3.select("body").append("div")
            .attr("class", "tooltip")
            .style("visibility", "hidden");
        this.tooltip(FTDrawer.default_tooltip_func);

        // Initialize DAG layout maker.
        let coordFunc = d3.coordVert();
        // Uncomment the code below to improve speed for larger graphs at the cost of appearance.
        /*if (ft_datahandler.nodes.length > 300) {
            coordFunc = d3.coordGreedy();
        }*/
        this.layout = d3.sugiyama()
            .nodeSize([120, 120])
            .layering(d3.layeringSimplex())
            .decross(d3.decrossOpt)
            .coord(coordFunc);

        // Defaults.  Can be overridden when calling new FamilyTree().
        this.orientation("horizontal");
        this.link_path(FTDrawer.default_link_path_func);
        this.node_label(FTDrawer.default_node_label_func);
        this.node_size(FTDrawer.default_node_size_func);
        this.node_class(FTDrawer.default_node_class_func);

        // Set starting position for root node.
        const default_pos = this.default_root_position();
        this.ft_datahandler.root.x0 = x0 || default_pos[0];
        this.ft_datahandler.root.y0 = y0 || default_pos[1];
    };

    default_root_position() {
        return [
            this.svg.attr("width") / 2,
            this.svg.attr("height") / 2
        ];
    }

    orientation(value) {
        // Getter/setter for tree orientation (horizontal/vertical).
        if (!value) {
            return this.orientation;
        } else {
            this._orientation = value;
            return this;
        }
    };

    node_separation(value) {
        // Getter/setter for separation of nodes in x and y direction (see d3-dag documentation).
        if (!value) {
            return this.layout.nodeSize();
        } else {
            this.layout.nodeSize(value);
            return this;
        }
    };

    layering(value) {
        // Getter/setter for layout operator (see d3-dag documentation).
        if (!value) {
            return this.layout.layering();
        } else {
            this.layout.layering(value);
            return this;
        }
    };

    decross(value) {
        // Getter/setter for descross operator (see d3-dag documentation).
        if (!value) {
            return this.layout.decross();
        } else {
            this.layout.decross(value);
            return this;
        }
    };

    coord(value) {
        // Getter/setter for coordinate operator (see d3-dag documentation).
        if (!value) {
            return this.layout.coord();
        } else {
            this.layout.coord(value);
            return this;
        }
    };

    static default_tooltip_func(node) {
        if (node.is_union()) {
            return;
        }
        const content = `
                <span style='margin-left: 2.5px;'><b>` + node.get_name() + `</b></span><br>
                <table style="margin-top: 2.5px;">
                        <tr><td><strong>Born</strong></td><td>` + print_year(node.get_birth_year()) + `</td></tr>
                        <tr><td><strong>Died</strong></td><td>` + print_year(node.get_death_year()) + `</td></tr>
                </table>
                `
        return content.replace(new RegExp("null", "g"), "?");
    };

    tooltip(tooltip_func) {
        // Setter for tooltips.
        if (!tooltip_func) {
            this.show_tooltips = false;
        } else {
            this.show_tooltips = true;
            this._tooltip_func = tooltip_func;
        }
        return this;
    };

    static default_node_label_func(node) {
        // Node label function. Not used in current setup.
        if (node.is_union()) {
            return;
        }
        return node.get_name();
    };

    node_label(node_label_func) {
        // Setter for node labels.
        if (node_label_func) {
            this.node_label_func = node_label_func;
        }
        return this;
    };

    static default_node_class_func(node) {
        // Returns a node's css classes as a string.
        if (node.is_union()) {
            return;
        }
        return "person";
    };

    node_class(node_class_func) {
        // Setter for node css class function.
        if (node_class_func) {
            this.node_class_func = node_class_func;
        }
        return this;
    };

    static default_node_size_func(node) {
        // Returns an integer determining the node's size.
        if (node.is_union()) {
            return 0;
        } else {
            return 10;
        }
    }

    node_size(node_size_func) {
        // Setter for node size function.
        if (node_size_func) {
            this.node_size_func = node_size_func;
        }
        return this;
    };

    static default_link_path_func(s, d) {
        function vertical_s_bend(s, d) {
            // Creates a diagonal curve fit for vertically oriented trees
            return `M ${s.x} ${s.y} 
            C ${s.x} ${(s.y + d.y) / 2},
            ${d.x} ${(s.y + d.y) / 2},
            ${d.x} ${d.y}`
        }

        function horizontal_s_bend(s, d) {
            // Creates a diagonal curve fit for horizontally oriented trees
            return `M ${s.x} ${s.y}
            C ${(s.x + d.x) / 2} ${s.y},
            ${(s.x + d.x) / 2} ${d.y},
            ${d.x} ${d.y}`
        }
        return this._orientation == "vertical" ? vertical_s_bend(s, d) : horizontal_s_bend(s, d);
    }

    link_path(link_path_func) {
        // Setter for link path function
        if (link_path_func) {
            this.link_path_func = link_path_func;
        }
        return this;
    }

    static make_unique_link_id(link) {
        return link.id || link.source.id + "_" + link.target.id;
    }

    draw(source = this.ft_datahandler.root) {
        // Get visible nodes and links.
        const nodes = this.ft_datahandler.dag.descendants(),
            links = this.ft_datahandler.dag.links();

        // Assign new x and y positions to all nodes.
        this.layout(this.ft_datahandler.dag);

        // Switch x and y coordinates if orientation = "horizontal".
        if (this._orientation == "horizontal") {
            let buffer = null;
            nodes.forEach(function(d) {
                buffer = d.x
                d.x = d.y;
                d.y = buffer;
            });
        }

        // ****************** Nodes section ***************************

        // Assign node data.
        let node = this.g.selectAll('g.node')
            .data(nodes, node => node.id)

        // Insert new nodes at the parent's previous position.
        let nodeEnter = node.enter().append('g')
            .attr('class', function (d) {
                const classes = ['node'];
                if (typeof d.data.gender !== 'undefined') {
                    classes.push('gender_' + d.data.gender);
                }
                if (typeof d.data.is_certain !== 'undefined') {
                    if (d.data.is_certain) {
                        classes.push('is_certain');
                    } else {
                        classes.push('is_uncertain');
                    }
                }
                return classes.join(' ');
            })
            .attr("transform", _ => "translate(" + source.x0 + "," + source.y0 + ")")
            .attr('visible', true);

        // Add tooltip.
        const tooltip_func = this._tooltip_func;
        nodeEnter
            .on("mouseover", function (event, d) {
                // Update tooltip div.
                $('.tooltipDiv').html(
                    '(Click to stop highlighting)<br><br>' +
                    tooltip_func(d)
                );
                // Reset opacity from previous mouseovers.
                nodeEnter.style("opacity", 1);
                linkEnter.style("opacity", 1);

                // Reduce opacity of all but parents, spouses, siblings, and children on mouseover.
                let mouseover_unions = [d.id, d.data.parent_union, ...d.data.own_unions];
                // Filter out null values.
                mouseover_unions = mouseover_unions.filter(i => i);
                nodeEnter.filter(function(f) {
                    if (f.constructor.name == 'Union') {
                        return !mouseover_unions.includes(f.id);
                    } else {
                        let filter_unions = [f.id, f.data.parent_union, ...f.data.own_unions];
                        filter_unions = filter_unions.filter(i => i);
                        return mouseover_unions.filter(
                            x => filter_unions.includes(x)
                        ).length == 0;
                    }
                })
                    .style("opacity", 0.2);
                linkEnter.filter(function(f) {
                     return !mouseover_unions.includes(f.source.id)
                         && !mouseover_unions.includes(f.target.id);
                })
                  .style("opacity", 0.2);
            })
            .on("click", function (d) {
                // If the node is for a person in DPRR, link to that person's page on click.
                if (typeof d.target.__data__.data.dprr_id !== 'undefined' && d.target.__data__.data.dprr_id !== null) {
                    window.open(
                        'http://romanrepublic.ac.uk/person/' + d.target.__data__.data.dprr_id + '/',
                        '_blank'
                    );
                }
            });

        // Add a circle for each uncertain/script-inserted node.
        d3.selectAll(".is_uncertain").append('circle')
            .attr('class', this.node_class_func)
            .attr('r', 1e-6)
            // Change the node's color so the selected person is highlighted.
            .attr('fill', d => {
                if (d.is_start) {
                    return "#FFFF00";
                } else if (typeof d.data.gender === 'undefined') {
                    return "#FFFFFF";
                } else if (d.data.gender == 'Male') {
                    return '#3EE6E3';
                } else {
                    return '#EB7FD5';
                }
                return "#FFFFFF";
            });

        // Add an icon for each certain/DPRR-provided node.
        d3.selectAll(".is_certain").insert("image")
            .attr('class', this.node_class_func)
            .attr('id', function(d) {
                return 'person_' + d.id + '_image';
            })
            .attr('height', 40)
            .attr('width', 24)
            // Offset the icon
            .attr('x', -12)
            .attr('y', -20)
            .attr("xlink:href", function(d) {
                if (d.is_start) {
                    return 'img/' + d.data.icon.slice(0, -4) + '_highlighted.png';
                }
                return 'img/' + d.data.icon;
            });

        // UPDATE
        let nodeUpdate = nodeEnter.merge(node);

        // Transition node to final coordinates.
        nodeUpdate
            .attr("transform", d => "translate(" + d.x + "," + d.y + ")");

        // Update node style.
        nodeUpdate.select('.node circle')
            .attr('r', this.node_size_func)
            .attr('class', this.node_class_func)
            // Only display a pointer if the link takes you to a website when clicked.
            .attr('cursor', d => {
                if (d.data.dprr_id !== 'undefined' && d.data.dprr_id !== null) {
                    return 'pointer';
                } else {
                    return 'default';
                }
            });

        // ****************** Links section ***************************

        // Update the links.
        let link = this.g.selectAll('path.' + this.link_css_class)
            .data(links, FTDrawer.make_unique_link_id);

        // Enter any new links at the parent's previous position.
        let linkEnter = link.enter().insert('path', "g")
            .attr("class", this.link_css_class)
            .attr("sourceId", d => d.source.id)
            .attr("targetId", d => d.target.id)
            .attr('d', _ => {
                let o = {
                    x: source.x0,
                    y: source.y0
                }
                return this.link_path_func(o, o)
            });

        // UPDATE
        let linkUpdate = linkEnter.merge(link);

        // Transition back to the parent element position
        linkUpdate
            .attr('d', d => this.link_path_func(d.source, d.target));

        // Expanding a big subgraph moves the entire dag out of the screen
        // to prevent this, cancel any transformations in y-direction.
        this.svg
            .call(
                this.zoom.transform,
                d3.zoomTransform(this.g.node()).translate(-(source.x - source.x0), -(source.y - source.y0)),
            );

        // General svg behavior
        $('svg, .infoDiv').on('click', function(event) {
            // Do not count clicking on a node to see more information as clicking to reset the screen.
            if (event.target.__data__ && event.target.__data__.data.dprr_id) {
                return;
            }
            $('.tooltipDiv').html(`Mouse over a person to see who they are and to highlight their immediate family,
                or click on them to view the DPRR page, if any.`);
            nodeEnter.style("opacity", 1);
            linkEnter.style("opacity", 1);
        });

        // Store current node positions for next transition.
        nodes.forEach(function(d) {
            d.x0 = d.x;
            d.y0 = d.y;
        });

        // Add functionality to the person search/highlight option.
        /**
         * Sets up actions to trigger on person name autocomplete.
         *
         * @param {object} personJSON All possibilities for person autocomplete.
         */
        function setupAutocomplete(personJSON) {
            const renderItemFunction = function (ul, item) {
                // In autocomplete options, highlight all text matches.
                let newText = String(item.label).replace(
                    // Since our labels have special regex characters, replace them.
                    new RegExp(this.term.replace(/[.*+?^${}()|[\]\\]/g, '\\$&'), "gi"),
                    "<span class=\'ui-state-highlight\'>$&</span>"
                );

                return jQuery("<li></li>")
                    .data("item.autocomplete", item)
                    .append("<div>" + newText + "</div>")
                    .appendTo(ul);
            };

            let $personElem = jQuery("#highlightPerson").autocomplete({
                minLength: 3,
                source: personJSON,
                autoFocus: true,
                select: function(event, ui) {
                    clearHighlight();
                    highlightNode('person_' + ui.item.id + '_image');
                    return true;
                }
            });

            // Different versions of JQuery use different classes.  Ease updating by allowing all types.
            let personElemAutocomplete = $personElem.data("ui-autocomplete") || $personElem.data("autocomplete");
            if (personElemAutocomplete) {
                personElemAutocomplete._renderItem = renderItemFunction;
            }
        }

        /**
         * Adds autocomplete functionality and clear button functionality
         * on page load.
         */
        jQuery(document).ready(function($) {
            // Populate autocomplete dynamically based on this graph.
            peopleJSON = [];
            $('g.is_certain').each(function() {
                peopleJSON.push({
                    'id': this.__data__.data.id,
                    'value': this.__data__.data.name,
                    'label': this.__data__.data.name
                });
            });
            peopleJSON.sort(function(a, b) {
                return a.label.localeCompare(b.label);
            })
            setupAutocomplete(peopleJSON);
        });

        /**
         * Removes all highlighted icons.
         */
        function clearHighlight() {
            $('.is_certain > image').filter((i, d) => {
                return (
                    typeof d.href == 'string' && d.href.endsWith('_highlighted.png')
                ) || (
                    d.href.baseVal.endsWith('_highlighted.png')
                );
            }).each(function(index) {
                $(this).attr('href', $(this).attr('href').slice(0, -16) + '.png');
            });
        }

        /**
         * Highlights a given node.
         *
         * The html ID of the node to highlight.
         */
        function highlightNode(nodeID) {
            $('.is_certain > image#' + nodeID).each(function() {
                $(this).attr('href', $(this).attr('href').slice(0, -4) + '_highlighted.png');
            });
        }

        $('#clearSearch').on('click', clearHighlight);
    };

    clear() {
        this.g.selectAll("*").remove();
    }
};

/**
 * Class to handle specifics of drawing this Family Tree.
 */
class FamilyTree extends FTDrawer {
    constructor(data, svg) {
        // Try to read the query parameters to determine if a specific start node was selected.
        // Only use this value if it represents a person in the data.
        const urlParams = new URLSearchParams(location.search);
        const start_node = urlParams.get('start_node');
        let ft_datahandler = null;
        const persons = data.persons;
        const persons_lookup = new Set();
        for (const [key, value] of Object.entries(persons)) {
            persons_lookup.add(key);
        }
        if (start_node && persons_lookup.has(start_node)) {
            ft_datahandler = new FTDataHandler(data, start_node);
        } else {
            ft_datahandler = new FTDataHandler(data);
        }
        ft_datahandler.setup_parents();
        super(ft_datahandler, svg);
    };

    get_root() {
        return this.ft_datahandler.root;
    }

    wait_until_data_loaded(old_data, delay, tries, max_tries) {
        if (tries == max_tries) {
            return;
        } else {
            const new_data = window.data;
            if (old_data == new_data) {
                setTimeout(
                    _ => this.wait_until_data_loaded(old_data, delay, ++tries, max_tries),
                    delay,
                )
            } else {
                this.draw_data(new_data);
                return;
            }
        }
    }

    draw_data(data) {
        let x0 = null,
            y0 = null;
        if (this.root !== null) {
            [x0, y0] = [this.root.x0, this.root.y0];
        } else {
            [x0, y0] = this.default_root_position();
        }
        this.ft_datahandler = new FTDataHandler(data);
        this.root.x0 = x0;
        this.root.y0 = y0;
        this.clear();
        this.draw();
    }

    load_data(path_to_data) {
        const old_data = data,
            max_tries = 5,
            delay = 1000,
            file = document.createElement('script');
        let tries = 0;
        file.onreadystatechange = function() {
            if (this.readyState == 'complete') {
                this.wait_until_data_loaded(old_data, delay, tries, max_tries);
            }
        }
        file.onload = this.wait_until_data_loaded(old_data, delay, tries, max_tries);
        file.type = "text/javascript";
        file.src = path_to_data;
        document.getElementsByTagName("head")[0].appendChild(file)
    }
};
