## Introduction to the Project
This project consists of a number of experimental and experiential visualizations of the Roman Republic funeral from both a "close reading" and "distant reading" perspective.  All data is based off of the database at the Digital Prosopography of the Roman Republic project (frontend: http://romanrepublic.ac.uk/, backend http://romanrepublic.ac.uk/rdf/repositories/dprr/query).  Data is then processed through several pipelines to produce visualizations in SketchUp, d3.js, javascript via https://github.com/vasturiano/3d-force-graph, Google Charts, and Unity.  This project has been produced in large part through the UCLA DH Accelerator 2018-2019 program and a Digital Humanities 199/299 capstone course in Spring 2019.

## The DPRR Database and Queries
The Digital Prosopography of the Roman Republic database was produced at King's College London to contain known (and uncertain) information about as many men and women from the city of Rome between 509 BCE and roughly 100 CE. It provides a front end for searching and easy navigation.  This project relies on its backend RDF server, however.  Each of the below visualizations starts from a query or series of queries to the RDF server in SPARQL.  This project contains the queries made to create the data.

The results of these queries are then saved as CSV files, lightly cleaned by removing the first two rows, and then parsed by various scripts as described below to generate the visualizations.

The Explore feature of the backend server is vital to understanding the database structure.  A Person, for instance, does not know its relationships or political positions - instead, each RelationshipAssertion or PostAssertion (representing a political post) knows which Person they are about.

## The Visualizations and Their Production
The following sections outlines the different types of visualizations and the code and algorithms used to generate them.

NOTEs: All Python files were made in Python 3.6 and are untested with later versions.

TODO: Merge the python code from all visualizations to standardize it. Produce new algorithms for guessing links between people where they are not present in the database.  Consider statistical approaches and machine learning approaches to this final problem. Try to estimate the number of funerals not represented in the database based off of population estimates and senatorial lists.

### SketchUp Visualizations
(Subfolder: SketchUp)

This set of visualizations uses SketchUp and Ruby to produce representations of each funeral represented in the DPRR, as well as a "bar graph", in which each funeral appears.  This allows users to see how many funerals occurred per 25 year period in addition to the details of each funeral.  These visualizations use 2 levels of certainty, one based solely on DPRR and one attempting to fill in the gaps based on family names (those with the same family names are almost certainly related).

NOTE: The in situ visualizations assume a model of the forum from 160 BCE.  This SketchUp model is too large for GitHub and so not included here.  The plan is to have this available elsewhere.

Pipeline:
* Generate the CSV files from the DPRR backend.
* Remove the top 2 rows of each.
* Run funeral.py to produce the Ruby input file for either level of certainty.
* Run funeral.rb as a SketchUp plugin.
* Select one of the two visualization types, producing images.

TODO: Add hyperaggressive model, update data to match the later visualizations. Include women in the visualizations.

### Unity Visualizations
(Subfolder: Unity)

This set of visualizations improves upon the SketchUp ones above by adding a third, "hyperaggressive" model and allowing interaction with the visualizations in the RomeLab 3D world.  Instead of Ruby and SketchUp, this uses C# and Unity.  The base of the code was produced by a team at Ball State, though it has been heavily modified.  These visualizations link to family trees, described below.

NOTES: The SketchUp file used for the Unity mesh is too large for GitHub. The Unity project relies on being embedded in a specific Drupal server at hvwc.ucla.edu and cannot run otherwise. Online (multiplayer) modes rely on the Photon Unity Network.

Pipeline:
* Generate the CSV files from the DPRR backend.
* Remove the top 2 rows of each.
* Run UnityInputGenerator.py to produce the input file for either level of certainty.
* Run the Unity code to generate models.
* Save the models as prefabs to be used later.
* Upload the Unity code to the Drupal site.

TODO: Decouple Unity from Drupal. Set up online and offline modes. Update AllFunerals to use the newest data.

### Family Trees
(Subfolder: Family Trees)

This set of visualizations shows each person in the database within their family tree, using D3.js.  As with the Unity, there are 3 levels of certainty - each Unity funeral in each model is associated with a tree.

Pipeline:
* Generate the CSV files from the DPRR backend.
* Remove the top 2 rows of each.
* Run d3TreeGenerator.py to produce a tree for each person for either level of certainty.
* This uses TreeVertical.html (including D3.js) as a template.
* Each person appears in exactly 1 graph

TODO: Improve images and spacing.


### 2D Family Network Graphs
(Subfolder: Family Graphs)

This set of visualizations (build in D3.js) connects people in the database based on parent-child, husband-wife, and adoption relationships.  Each node has a pop-up on mouse-over for more info, is clickable to open the corresponding DPRR page, and is color-coded to show who is in the same gens (Roman family).

Pipeline:
* Generate PersonQuery and RelationshipQuery from the DPRR backend.
* Remove the top 2 rows of each.
* Run graphGenerator.py, which reads these files. graphGenerator uses graphTemplate.html (involving D3.js) as a building block to produce graphs
* Each person appears in exactly 1 graph

TODO: Increase readability for large graphs, consider using custom icons for famous people, add tentative connections where other visualizations add them, allow multiple graphs in the same window/file.

### 3D Visualizations
(Subfolder: 3D Visualizations)

This set of visualizations is a 3D version of the previous 2D Family Network Graphs visualizations.  Instead of using D3.js, this makes use of vasturiano's 3D Force Graph package, built in Javascript and using WebGL.  It retains all the features of the 2D version while being easier to read and navigate.

Pipeline:
* Generate PersonQuery and RelationshipQuery from the DPRR backend.
* Remove the top 2 rows of each.
* Run 3d_generator.py, which reads these files. 3d_generator uses 3D_Template.html as a building block to produce graphs
* Each person appears in exactly 1 graph

TODO: As above, plus modifty the package for easier navigation and different actions on click.  Update UI to better describe what to do.
