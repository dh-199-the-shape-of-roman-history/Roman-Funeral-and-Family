## Introduction to the Project
This set of visualizations (build in D3.js) displays all known funerals, sorted by time period and by number of people involved.  Each node has a pop-up on mouse-over for more info and is clickable to open the corresponding DPRR page.

Pipeline:
* (If desired) Refresh DPRR data by going to Funerals_2022/py and running `python 00_update_server_data.py`
* (If desired) Add manual overrides in Funerals/2022/json/override_data.json, then go to Funerals_2022/py and run `python 01_add_manual_overrides.py`
* (If changes made above) Go to Funerals_2022/py and run `python 02_d3_data_generator.py`
* Open Funerals_2022/funerals.html.

## Project Structure
### funerals.html
This file contains the HTML structure for the visualization.  The visualization is injected by JavaScript.
The filters use Bootstrap's grid layout for responsiveness.  IDs are referred to by JavaScript, so changes here need to be matched in the JavaScript files.

### css/funerals.css
Custom stylesheet for the visualization.  Sets padding and spacing for most larger aspects, but not each individual funeral.

### img/
Contains iconography, including highlighted images.  If sizes change, visualization.js needs to change its placement algorithm accordingly.
Files must be .png, and each normal icon must have a highlighted equivalent ending in _highlighted.png.

### js/autocomplete.js
Contains code setting up the autocomplete and clear functionality for person and gens search.  Can change autocomplete behavior here.
Uses jQuery and jQuery UI's autocomplete module.

### js/database_data.js
Automatically produced as part of the above pipeline.  Do not edit.

### js/visualization.js
Contains code for creating the visualization and changing models.  Spacing for funerals and eras, as well as zoom/pan behavior, is set here.
Uses jQuery and D3.js.

### json/combined_data.json
Contains data from the DPRR server combined with any manual overrides.  Do not edit.

### json/database_data.json
Contains data from the DPRR server.  Do not edit.

### json/override_data.json
Enter any manual overrides here.  Structure is a list of objects, where each object represents one person to override.  The following is an example of overriding
the father for person 3 to be person 2072.  A source/reason is supported, though not shown in the visualization.
```
[
    {
        "id": "3",
        "fatherID": {
            "source": "Fake values for demo",
            "value": "2072"
        },
        "grandfatherID": {
            "source": "Testing results",
            "value": "2072"
        }
    }
]
```

### py/00_update_server_data.py
Queries the DPRR server for data and writes it to json/database_data.json.  You can change the query to get more information here.

### py/01_add_manual_overrides.py
Overrides data in json/database_data.json with data in json/override_data.json, and writes it to json/combined_data.json.
If overrides should be noted in the visualization, edit this file and the underlying JS visualization.

### py/02_d3_data_generator.py
Takes data from json/combined_data.json and writes required JSON structures to js/database_data.js.
Set to ignore funerals after 100 CE, which may be changed by editing the value in `FINAL_YEAR` at the top of the file.
Does not display ancestors with no high positions; this and the gap filling algorithms can be altered here.
