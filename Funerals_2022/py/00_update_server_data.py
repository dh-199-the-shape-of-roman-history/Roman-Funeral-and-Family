"""
Queries the DPRR server and saves the results as JSON.

Author: Benjamin Niedzielski (benjamin_niedzielski@alumni.brown.edu)
Last Modified: June 26, 2022
"""

# Imports
import requests
import json

# Constants
ENDPOINT = 'http://romanrepublic.ac.uk/rdf/endpoint'
WRITE_PATH = '../json/database_data.json'


# The RDF server takes a SPARQL query.
# SPARQL is a graph database where OPTIONAL is roughly equivalent to LEFT JOIN.
# An OPTIONAL group will only return data if all fields in it are not null.
query = """
PREFIX : <http://romanrepublic.ac.uk/rdf/ontology#>
PREFIX owl: <http://www.w3.org/2002/07/owl#>
PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
PREFIX xml: <http://www.w3.org/XML/1998/namespace>
PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>
PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
PREFIX vocab: <http://romanrepublic.ac.uk/rdf/ontology#>
PREFIX map: <http://romanrepublic.ac.uk/rdf/entity/#>
PREFIX db: <http://romanrepublic.ac.uk/rdf/entity/>

SELECT DISTINCT
    ?id
    ?name
    ?nomen
    ?cognomen
    ?highestOffice
    ?birth
    ?death
    ?triumphator
    ?fatherID
    ?fatherIsUncertain
    ?grandfatherID
    ?grandfatherIsUncertain
WHERE {
    ?aperson a vocab:Person;
        vocab:isSex <http://romanrepublic.ac.uk/rdf/entity/Sex/Male>;
        vocab:hasID ?id;
        vocab:hasName ?name;
    OPTIONAL {
        ?aperson a vocab:Person;
            vocab:hasNomen ?nomen;
    }
    OPTIONAL {
        ?aperson a vocab:Person;
            vocab:hasCognomen ?cognomen;
    }
    OPTIONAL {
        ?aperson a vocab:Person;
            vocab:hasEraFrom ?birth;
    }
    OPTIONAL {
        ?aperson a vocab:Person;
            vocab:hasEraTo ?death;
    }
    OPTIONAL {
        ?aperson a vocab:Person;
            vocab:hasHighestOffice ?highestOffice.
    }
    OPTIONAL {
        ?postAssert a vocab:PostAssertion;
            vocab:hasOffice ?office;
            vocab:isAboutPerson ?aperson.
        FILTER (?office = <http://romanrepublic.ac.uk/rdf/entity/Office/260>)
        BIND ('True' as ?triumphator)
    }
    OPTIONAL {
        ?relAssert a vocab:RelationshipAssertion;
            vocab:isAboutPerson ?aperson;
            vocab:hasRelationship <http://romanrepublic.ac.uk/rdf/entity/Relationship/5>;
            vocab:hasRelatedPerson ?relat.
        ?relat a vocab:Person;
            vocab:isSex <http://romanrepublic.ac.uk/rdf/entity/Sex/Male>;
            vocab:hasID ?fatherID.
    }
    OPTIONAL {
        ?relAssert a vocab:RelationshipAssertion;
            vocab:isAboutPerson ?aperson;
            vocab:hasRelationship <http://romanrepublic.ac.uk/rdf/entity/Relationship/5>;
            vocab:isUncertain ?fatherIsUncertain;
            vocab:hasRelatedPerson ?relat.
        ?relat a vocab:Person;
            vocab:isSex <http://romanrepublic.ac.uk/rdf/entity/Sex/Male>.
    }
    OPTIONAL {
        ?relAssert a vocab:RelationshipAssertion;
            vocab:isAboutPerson ?aperson;
            vocab:hasRelationship <http://romanrepublic.ac.uk/rdf/entity/Relationship/19>;
            vocab:hasRelatedPerson ?relat.
        ?relat a vocab:Person;
            vocab:isSex <http://romanrepublic.ac.uk/rdf/entity/Sex/Male>;
            vocab:hasID ?grandfatherID.
    }
    OPTIONAL {
        ?relAssert a vocab:RelationshipAssertion;
            vocab:isAboutPerson ?aperson;
            vocab:hasRelationship <http://romanrepublic.ac.uk/rdf/entity/Relationship/19>;
            vocab:isUncertain ?grandfatherIsUncertain;
            vocab:hasRelatedPerson ?relat.
        ?relat a vocab:Person;
            vocab:isSex <http://romanrepublic.ac.uk/rdf/entity/Sex/Male>.
    }
}
ORDER BY ?id
"""  # noqa: E501 - Ignore line length code quality requirements.

results = requests.get(
    ENDPOINT,
    params={'query': query, 'format': 'application/json'}
)
# If the server is down, raise an exception and end.
results.raise_for_status()

results_json = results.json()
keys = results_json['head']['vars']
results_json = results_json['results']['bindings']

new_results_json = []
for person in results_json:
    person_dict = {}
    for key in keys:
        person_key = person.get(key, {'value': None})
        person_dict[key] = person_key['value']
    new_results_json.append(person_dict)

with open(WRITE_PATH, 'w', encoding='utf-8') as f:
    json.dump(new_results_json, f, ensure_ascii=False, indent=4)
