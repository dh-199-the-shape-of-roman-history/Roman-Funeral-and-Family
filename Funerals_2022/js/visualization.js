/**
 * Contains the logic for producing and modifying the D3 visualization.
 *
 * Author: Benjamin Niedzielski (benjamin_niedzielski@alumni.brown.edu)
 * Last Modified: June 27, 2022
 */

// Set the dimensions and margins of the diagram
let margin = {top: 20, right: 20, bottom: 0, left: 20},
    width = document.getElementById('allFunerals').clientWidth - margin.left - margin.right,
    height = Math.min(700, window.innerHeight - 124);
let svg = null;
let div = null;

let funeralCountForEra = 0;
let eraCount = 0;
let maxColumnWidth = 0;
let startingWidth = 0;
let maxDivWidth = 0;
let g = null;
let funeralG = null;

let defaultZoom = d3.zoomIdentity;
defaultZoom.x = margin.left;
defaultZoom.y = margin.top;

let zoomScreenSizeMulti = 1.0;

window.addEventListener("resize", resizeVisualization);

setupDisplayDiv();
conservativeFuneralData.timePeriods.forEach(setFunerals);
// Set up the default/maximum zoom to fit all columns in the screen at once.
defaultZoom.k = width / startingWidth;
let currentZoom = defaultZoom;
setZoom();

/**
 * Based on the ratio between the old window size and the new one,
 * resize the dimensions of the svg to scale the visualization.
 */
function resizeVisualization() {
    oldWidth = width;
    oldHeight = height;
    width = document.getElementById('allFunerals').clientWidth - margin.left - margin.right,
    height = Math.min(700, window.innerHeight - 124);

    let newScale = (width * 1.0 / oldWidth);
    zoomScreenSizeMulti *= newScale;
    currentZoom.x *= newScale;
    currentZoom.y *= newScale;
    currentZoom.k *= newScale;

    d3.select("#allFunerals svg")
        .attr("width", width + margin.right + margin.left - 20)
        .attr("height", height + margin.top + margin.bottom)

    setZoom();
}

/**
 * Given the empty div with ID allFunerals,
 * adds the structure that will hold the visualization.
 *
 * Also resets global constants used for building the visualization.
 */
function setupDisplayDiv() {
    jQuery('#allFunerals').empty();

    svg = d3.select("#allFunerals")
                .append("svg")
                .attr("width", width + margin.right + margin.left - 20)
                .attr("height", height + margin.top + margin.bottom)
                .append("g")
                .attr("transform", "translate(" + margin.left + "," + margin.top + ")")
                .attr('id', 'allFuneralsGroup');

    div = d3.select("#allFunerals").append("div")
        .attr("class", "tooltip")
        .attr("id", "personTooltip")
        .style("opacity", 0);

    funeralCountForEra = 0;
    eraCount = 0;
    maxColumnWidth = 0;
    startingWidth = 0;
    maxDivWidth = 0;
    g = null;
    funeralG = null;
}

/**
 * Sets the ancestors for a given funeral, one by one.
 *
 * @param {object} data The JSON data for this funeral.
 */
function setPoints(data) {
    // Track the number of models in this funeral for proper spacing.
    count = 0;
    funeralG.selectAll("indPoints")
        .data(data)
        .enter()
        .insert("image")
            .attr('class', 'node')
            .attr('gens', function(data) {
                return data.nomen;
            })
            .attr('personID', function(data) {
                return data.id;
            })
            // Not presently used but available for selection.
            .attr('eraCount', eraCount)
            .attr('funeralNumber', funeralCountForEra)
            .attr('x', function(data) {
                // Adjust globals witin this function.
                // A cleaner solution would be preferable.
                count++;
                funeralG.attr('peopleIncluded', funeralG.attr('peopleIncluded') + data.id + '_')
                return startingWidth + count * 40;
            })
            .attr('y', function(data) {
                return 20 + funeralCountForEra * 60;
            })
            .attr('height', 40)
            .attr('width', 24)
            .attr("xlink:href", function(d) {
                return 'img/' + d.icon;
            })
            .on('click', function(event, data) {
                window.open(
                    'http://romanrepublic.ac.uk/person/' + data.id + '/',
                    '_blank'
                );
            })
            .on("mouseover", function(event, data) {
                div.transition()
                    .duration(200)
                    .style("opacity", .65);
                div.html("<span>" + data.name +
                        "</span><br><strong>Born:</strong> <span>" + data.birthDate +
                        "</span> <strong>Died:</strong> <span>" + data.deathDate +
                        "</span><br><strong>Highest Position:</strong> <span>" + data.bestPos + "</span>")
                     // Ensure the tooltip does not get cut off by the edge of the screen.
                    .style("left", (event.pageX - 300 > 0 ? event.pageX - 300 : event.pageX) + "px")
                    .style("top", (event.pageY - document.getElementById('personTooltip').clientHeight) + "px");
            })
            .on("mouseout", function(data) {
                div.transition()
                    .duration(200)
                    .style("opacity", 0);
            });
}

/**
 * Creates a group for the new funeral, updating counts and sizes accordingly.
 *
 * @param {object} data The json data for this funeral.
 */
function setFuneral(data) {
    funeralG = g.append('g')
        .attr('class', 'funeralGroupInner')
        .attr('funeralEra', eraCount)
        .attr('funeralNumber', funeralCountForEra)
        .attr('peopleIncluded', '_');
    setPoints(data);
    funeralCountForEra++;
    if (count * 40 > maxColumnWidth) {
        maxColumnWidth = count * 40;
    }
}

/**
 * Creates a group for this 25 year period and updates spacing accordingly.
 *
 * @param {object} data The json data for this period's funerals.
 */
function setFunerals(data) {
    funeralCountForEra = 0;
    maxColumnWidth = 0;
    maxDivWidth = 0;
    g = svg.append('g').attr('class', 'funeralGroup')
        .attr('timePeriod', data.timePeriod)
        .attr('eraCount', eraCount);
    g.insert('text').text(data.timePeriod).attr('x', startingWidth + 30);
    data.funerals.forEach(setFuneral);
    startingWidth += (maxColumnWidth + 90);
    eraCount++;
}

/**
 * Add zoom and pan capabilities to visualization elements.
 */
function setZoom() {
    zoom = d3.zoom()
       	.scaleExtent([defaultZoom.k * zoomScreenSizeMulti, 2 * zoomScreenSizeMulti])
       	.on("zoom", handleZoom);
    d3.select("svg")
        .call(zoom);
    d3.select("svg g")
        .attr("transform", currentZoom);
}

/**
 * This function alters D3.js graphics based on zoom and pan events.
 *
 * @param e The transformation event.
 */
function handleZoom(e) {
    // Limit panning so the visualization remains on the screen.
    // TODO: Redo this as part of the d3.zoom() call above: http://bl.ocks.org/garrilla/11280861.
    if (e.transform.x > width * 0.5) {
        e.transform.x = width * 0.5;
    }
    if (e.transform.y > height * 0.05) {
        e.transform.y = height * 0.05;
    }
    currentZoom = e.transform;
    d3.select("svg g")
        .attr("transform", e.transform);
}

/**
 * Handle user selection of model type.
 *
 * TODO: Improve performance.
 */
jQuery('select#algorithm').on('change', function() {
    setupDisplayDiv();

    switch (this.value) {
        case 'conservative':
            conservativeFuneralData.timePeriods.forEach(setFunerals);
            break;
        case 'aggressive':
            aggressiveFuneralData.timePeriods.forEach(setFunerals);
            break;
        case 'hyperaggressive':
            hyperaggressiveFuneralData.timePeriods.forEach(setFunerals);
            break;
    }

    // Set up the default/maximum zoom to fit all columns in the screen at once.
    defaultZoom.k = width / startingWidth;
    zoomScreenSizeMulti = 1.0;
    currentZoom = defaultZoom;

    setZoom();

    // Preserve the current filter, if any, based on the hidden ID that tracks these filters.
    selectedID = jQuery("#searchedID").html();
    selectedGensID = jQuery("#searchedGensID").html();
    if (selectedID) {
        jQuery("g.funeralGroupInner:not([peopleIncluded*='_" + selectedID + "_'])").addClass('notFocused');
        jQuery("image[personID='" + selectedID + "']").each(function(index) {
            jQuery(this).addClass('highlighted');
            jQuery(this).attr('href', function() {
                let oldHREF = jQuery(this).attr('href');
                if (!oldHREF.includes('highlighted')) {
                    return oldHREF.substring(0, oldHREF.length - 4) + '_highlighted.png';
                }
                return oldHREF;
            });
            return true;
        });
    } else if (selectedGensID) {
        jQuery("g, g *").removeClass('notFocused');
        jQuery("image:not([gens='" + selectedGensID + "'])").addClass('notFocused');
    }
});
