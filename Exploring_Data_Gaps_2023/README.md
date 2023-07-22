# Exploring Data Gaps 2023

Using [Benjamin W. Portner's js_family_tree package](https://github.com/BenPortner/js_family_tree) as a starting point,
allows users to see family trees of Romans in DPRR, up to those born in 130 BCE.

Fills in missing parents, which is a first step towards giving women more visibility
in these models.

Start from ./index.html and select a person to view.

Author: Benjamin E. Niedzielski

License: GNU General Public License v3.0

## Data format

The data files contain a single javascript object, which represents the family tree. The fields are as follows:

- `start`: Enter here the id of the person, which should be the starting point of the family tree.
- `persons`: Contains metadata about each person. Make sure each `id` is unique. Also, make sure each element in `own_unions` refers to a valid `union` defined in `data.unions`.
- `unions`: Contains metadata about each family. Each entry in `partner` and `children` must refer to a valid `person` defined in `data.persons`.
- `links`: Defines how unions and persons are to be linked (edge list). Basically an alternative representation of the information contained in `person.own_unions`, `union.partner` and `union.children`. Unfortunately, this redundancy is for now necessary to ensure proper functioning of the code!

```javascript
data = {
    "start":"id4",
    "persons": {
        "id1": { "id": "id1", "name": "Adam", "birthyear": 1900, "deathyear": 1980, "own_unions": ["u1"], "birthplace":"Alberta", "deathplace":"Austin"},
        "id2": { "id": "id2", "name": "Berta", "birthyear": 1901, "deathyear": 1985, "own_unions": ["u1"], "birthplace":"Berlin", "deathplace":"Bern" },
        "id3": { "id": "id3", "name": "Charlene", "birthyear": 1930, "deathyear": 2010, "own_unions": ["u3", "u4"], "parent_union": "u1", "birthplace":"Château", "deathplace":"Cuxhaven" },
        "id4": { "id": "id4", "name": "Dan", "birthyear": 1926, "deathyear": 2009, "own_unions": [], "parent_union": "u1", "birthplace":"den Haag", "deathplace":"Derince" },
        "id5": { "id": "id5", "name": "Eric", "birthyear": 1931, "deathyear": 2015, "own_unions": ["u3"], "parent_union": "u2", "birthplace":"Essen", "deathplace":"Edinburgh" },
        "id6": { "id": "id6", "name": "Francis", "birthyear": 1902, "deathyear": 1970, "own_unions": ["u2"], "birthplace":"Firenze", "deathplace":"Faizabad" },
        "id7": { "id": "id7", "name": "Greta", "birthyear": 1905, "deathyear": 1990, "own_unions": ["u2"] },
        "id8": { "id": "id8", "name": "Heinz", "birthyear": 1970, "own_unions": ["u5"], "parent_union": "u3" },
        "id9": { "id": "id9", "name": "Iver", "birthyear": 1925, "deathyear": 1963, "own_unions": ["u4"] },
        "id10": { "id": "id10", "name": "Jennifer", "birthyear": 1950, "own_unions": [], "parent_union": "u4" },
        "id11": { "id": "id11", "name": "Klaus", "birthyear": 1933, "deathyear": 2013, "own_unions": [], "parent_union": "u1" },
        "id12": { "id": "id12", "name": "Lennart", "birthyear": 1999, "own_unions": [], "parent_union": "u5" },
    },
    "unions": {
        "u1": { "id": "u1", "partner": ["id1", "id2"], "children": ["id3", "id4", "id11"] },
        "u2": { "id": "u2", "partner": ["id6", "id7"], "children": ["id5"] },
        "u3": { "id": "u3", "partner": ["id3", "id5"], "children": ["id8"] },
        "u4": { "id": "u4", "partner": ["id3", "id9"], "children": ["id10"] },
        "u5": { "id": "u5", "partner": ["id8"], "children": ["id12"] },
    },
    "links": [
        ["id1", "u1"],
        ["id2", "u1"],
        ["u1", "id3"],
        ["u1", "id4"],
        ["id6", "u2"],
        ["id7", "u2"],
        ["u2", "id5"],
        ["id3", "u3"],
        ["id5", "u3"],
        ["u3", "id8"],
        ["id3", "u4"],
        ["id9", "u4"],
        ["u4", "id10"],
        ["u1", "id11"],
        ["id8", "u5"],
        ["u5", "id12"],
    ]
}
```

## End to End Setup
While this repository comes with the data ready to run, you can refresh and update data as follows.
1. Navigate to the python folder.
2. Run ```python 00_update_server_data.py``` to query DPRR for the initial data.
3. Run ```python 01_produce_family_tree.py``` to generate family trees and fill gaps.
4. Navigate up one directory, to Explore-Data-Gaps-2023.
5. Open ```index.html``` and select a person.
6. A tree will open in a new tab, with your choice highlighted.  You may mouse over a person to see more details and
their immediate family and click on users to see their DPRR page, if any. Clicking elsewhere will deselect your choice.
You may also choose to highlight another person, although the screen will not recenter on them.