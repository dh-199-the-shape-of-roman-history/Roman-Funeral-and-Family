## Introduction to the Project
This project consists of a number of experimental and experiential visualizations of the Roman Republic funeral from both a "close reading" and "distant reading" perspective.  All data is based off of the database at the Digital Prosopography of the Roman Republic project (frontend: http://romanrepublic.ac.uk/, backend http://romanrepublic.ac.uk/rdf/repositories/dprr/query).  Data is then processed through several pipelines to produce visualizations in SketchUp, d3.js, javascript via https://github.com/vasturiano/3d-force-graph, Google Charts, and Unity.  This project has been produced in large part through the UCLA DH Accelerator 2018-2019 program and a Digital Humanities 199/299 capstone course in Spring 2019.

## The DPRR Database and Queries
The Digital Prosopography of the Roman Republic database was produced at King's College London to contain known (and uncertain) information about as many men and women from the city of Rome between 509 BCE and roughly 100 CE. It provides a front end for searching and easy navigation.  This project relies on its backend RDF server, however.  Each of the below visualizations starts from a query or series of queries to the RDF server in SPARQL.  This project contains the queries made to create the data.

The results of these queries are then saved as CSV files, lightly cleaned by removing the first two rows, and then parsed by various scripts as described below to generate the visualizations.

The Explore feature of the backend server is vital to understanding the database structure.  A Person, for instance, does not know its relationships or political positions - instead, each RelationshipAssertion or PostAssertion (representing a political post) knows which Person they are about.

## The Visualizations and Their Production
The following sections outlines the different types of visualizations and the code and algorithms used to generate them.

### 2D Family Network Graphs
