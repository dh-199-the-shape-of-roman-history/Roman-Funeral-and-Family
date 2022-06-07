"""
Adds user overrides to person data from the DPRR RDF server.

Author: Benjamin Niedzielski (benjamin_niedzielski@alumni.brown.edu)
Last Modified: June 26, 2022
"""

# Imports
import json

# Constants
SERVER_DATA_PATH = '../json/database_data.json'
OVERRIDE_DATA_PATH = '../json/override_data.json'
WRITE_PATH = '../json/combined_data.json'


with open(SERVER_DATA_PATH, 'r', encoding='utf-8') as f:
    database_results_json = json.load(f)

with open(OVERRIDE_DATA_PATH, 'r', encoding='utf-8') as f:
    override_results_json = json.load(f)

new_override_results = {}
for person in override_results_json:
    person_json = {}
    id = person['id']
    for key in person.keys():
        if key != 'id':
            person_json[key] = person[key]['value']
    new_override_results[id] = person_json

full_data_dict = {}
for person in database_results_json:
    id = person['id']
    full_data_dict[id] = person
    # Use override values where present.
    if id in new_override_results.keys():
        for key in new_override_results[id]:
            full_data_dict[id][key] = new_override_results[id][key]

full_data_list = []
for person in full_data_dict.values():
    full_data_list.append(person)

with open(WRITE_PATH, 'w', encoding='utf-8') as f:
    json.dump(full_data_list, f, ensure_ascii=False, indent=4)
