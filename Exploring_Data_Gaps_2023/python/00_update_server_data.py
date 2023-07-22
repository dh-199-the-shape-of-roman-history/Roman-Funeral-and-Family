"""
Queries the DPRR server and saves the results as JSON.

Author: Benjamin Niedzielski (benjamin_niedzielski@alumni.brown.edu)
Last Modified: June 26, 2022
"""

# Imports
import requests
import sqlite3

# Constants
ENDPOINT = 'http://romanrepublic.ac.uk/rdf/endpoint'
DATABASE_LOCATION = 'roman_prosopography.db'

# The RDF server takes a SPARQL query.
# SPARQL is a graph database where OPTIONAL is roughly equivalent to LEFT JOIN.
# An OPTIONAL group will only return data if all fields in it are not null.
PEOPLE_QUERY = """
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
    ?gender
    ?highestOffice
    ?birth
    ?death
WHERE {
    ?aperson a vocab:Person;
        vocab:hasID ?id;
        vocab:hasName ?name;
        vocab:isSex ?gender;
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
}
ORDER BY ?id
"""  # noqa: E501 - Ignore line length code quality requirements.

RELATIONSHIP_QUERY = """
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
	?assertion
	?aboutPerson
	?relatedPerson
	?relationship 
	?relationshipID
	?relationshipName
WHERE {
    ?assertion a vocab:RelationshipAssertion;
		vocab:isAboutPerson ?aboutPerson;
		vocab:hasRelatedPerson ?relatedPerson;
		vocab:hasRelationship ?relationship.
    ?relationship a vocab:Relationship;
        vocab:hasID ?relationshipID;
        vocab:hasName ?relationshipName;
}
"""  # noqa: E501 - Ignore line length code quality requirements.

TRIUMPHATOR_QUERY = """
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
    ?personId
	?triumphator
WHERE {
    ?postAssert a vocab:PostAssertion;
        vocab:hasOffice ?office;
        vocab:isAboutPerson ?person.
    	FILTER (?office = <http://romanrepublic.ac.uk/rdf/entity/Office/260>)
    	BIND ('True' as ?triumphator)
    ?person a vocab:Person;
        vocab:hasID ?personId
}
"""  # noqa: E501 - Ignore line length code quality requirements.


def dict_factory(cursor, row):
    """Allows sqlite queries to return a dict.
    Unspecified behavior when joins result in multiple columns with the same name."""
    fields = [column[0] for column in cursor.description]
    return {key: value for key, value in zip(fields, row)}


def setupDatabase(cur):
    """Create the necessary database tables, if they do not exist."""
    cur.execute("""
        CREATE TABLE IF NOT EXISTS person(
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            name TEXT,
            nomen TEXT,
            cognomen TEXT KEY,
            gender TEXT KEY,
            highest_office TEXT,
            birth INTEGER,
            death INTEGER,
            dprr_id INTEGER KEY,
            source TEXT KEY,
            is_certain TINYINT KEY
        );
    """)
    cur.execute("""
        CREATE TABLE IF NOT EXISTS relationship_type(
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            name TEXT
        );
        """)
    cur.execute("""
        CREATE TABLE IF NOT EXISTS relationship(
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            about_person INTEGER KEY,
            related_person INTEGER KEY,
            type INTEGER KEY,
            dprr_id INTEGER KEY,
            source TEXT KEY,
            FOREIGN KEY(type) REFERENCES relationship_type(id),
            FOREIGN KEY(about_person) REFERENCES person(id),
            FOREIGN KEY(related_person) REFERENCES person(id)
        );
    """)
    cur.execute("""
        CREATE TABLE IF NOT EXISTS triumphator(
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            about_person INTEGER KEY,
            source TEXT KEY,
            FOREIGN KEY(about_person) REFERENCES person(id)
        );
    """)


def upsertPeople(cur):
    """
    Queries DPRR for people data and inserts or updates them into the database.
    :param cur: The sqlite3 cursor to use to update the database.
    """
    results = requests.get(
        ENDPOINT,
        params={'query': PEOPLE_QUERY, 'format': 'application/json'}
    )
    # If the server is down, raise an exception and end.
    results.raise_for_status()

    results_json = results.json()
    keys = results_json['head']['vars']
    results_json = results_json['results']['bindings']

    for person in results_json:
        person_dict = {}
        for key in keys:
            person_key = person.get(key, {'value': None})
            person_dict[key] = person_key['value']
        try:
            # Prepare person for database
            person_id = int(person_dict.get('id', False))
            name = person_dict.get('name', None)
            nomen = person_dict.get('nomen', None)
            cognomen = person_dict.get('cognomen', None)
            gender = person_dict.get('gender', None)
            if gender is not None and 'Male' in gender:
                gender = 'Male'
            elif gender is not None and 'Female' in gender:
                gender = 'Female'
            highest_office = person_dict.get('highestOffice', None)
            birth = person_dict.get('birth', None)
            if birth is not None:
                birth = int(birth)
            death = person_dict.get('death', None)
            if death is not None:
                death = int(death)
            # See if we need to insert or update an existing row.
            res = cur.execute('SELECT * FROM person WHERE dprr_id = ?', (person_id,))
            if len(res.fetchall()) > 0:
                cur.execute(
                    """
                    UPDATE person
                    SET name=?, nomen=?, cognomen=?, gender=?, highest_office=?, birth=?, death=?, source="DPRR",
                        is_certain=1
                    WHERE dprr_id=?
                    """,
                    (name, nomen, cognomen, gender, highest_office, birth, death, person_id)
                )
            else:
                cur.execute(
                    """
                    INSERT INTO person (name, nomen, cognomen, gender, highest_office, birth, death, source, dprr_id,
                        is_certain)
                    VALUES (?, ?, ?, ?, ?, ?, ?, "DPRR", ?, 1)
                    """,
                    (name, nomen, cognomen, gender, highest_office, birth, death, person_id)
                )
        except Exception as e:
            print(f'Error inserting person {person} into database: {e}')


def upsertRelationships(cur):
    """
    Queries DPRR for relationship data and inserts or updates them into the database.
    :param cur: The sqlite3 cursor to use to update the database.
    """
    results = requests.get(
        ENDPOINT,
        params={'query': RELATIONSHIP_QUERY, 'format': 'application/json'}
    )
    # If the server is down, raise an exception and end.
    results.raise_for_status()

    results_json = results.json()
    keys = results_json['head']['vars']
    results_json = results_json['results']['bindings']

    for relationship in results_json:
        relationship_dict = {}
        for key in keys:
            relationship_key = relationship.get(key, {'value': None})
            relationship_dict[key] = relationship_key['value']
        try:
            # Prepare relationship for entry by converting DPRR ids to our database's ids.
            relationship_id = relationship_dict.get('assertion', '')
            relationship_id = int(relationship_id.split('/')[-1])
            about_person_id = relationship_dict.get('aboutPerson', '')
            about_person_id = int(about_person_id.split('/')[-1])
            related_person_id = relationship_dict.get('relatedPerson', '')
            related_person_id = int(related_person_id.split('/')[-1])
            relationship_type_id = relationship_dict.get('relationshipID', None)
            if relationship_type_id is not None:
                relationship_type_id = int(relationship_type_id)
            relationship_type_name = relationship_dict.get('relationshipName', None)
            res = cur.execute('SELECT * FROM person WHERE dprr_id IN (?, ?)', (about_person_id, related_person_id))
            about_person_db_id = None
            related_person_db_id = None
            for row in res.fetchall():
                if row['dprr_id'] == about_person_id:
                    about_person_db_id = row['id']
                if row['dprr_id'] == related_person_id:
                    related_person_db_id = row['id']
            if about_person_db_id is None or related_person_db_id is None:
                print(f'Error looking up people for relationship {relationship}')
                continue
            # Determine if we should insert or update the relationship_type and relationship.
            res = cur.execute('SELECT * FROM relationship_type WHERE id = ?', (relationship_type_id,))
            if len(res.fetchall()) == 0:
                cur.execute(
                    'INSERT INTO relationship_type (id, name) VALUES (?, ?)',
                    (relationship_type_id, relationship_type_name)
                )
            res = cur.execute('SELECT * FROM relationship WHERE dprr_id = ?', (relationship_id,))
            if len(res.fetchall()) > 0:
                cur.execute(
                    """
                    UPDATE relationship
                    SET about_person=?, related_person=?, type=?, source="DPRR"
                    WHERE dprr_id=?
                    """,
                    (about_person_db_id, related_person_db_id, relationship_type_id, relationship_id)
                )
            else:
                cur.execute(
                    """
                    INSERT INTO relationship (about_person, related_person, type, source, dprr_id)
                    VALUES (?, ?, ?, "DPRR", ?)
                    """,
                    (about_person_db_id, related_person_db_id, relationship_type_id, relationship_id)
                )
        except Exception as e:
            print(f'Error inserting relationship {relationship} into database: {e}')


def upsertTriumphators(cur):
    """
    Queries DPRR for triumphator data and inserts or updates them into the database.
    :param cur: The sqlite3 cursor to use to update the database.
    """
    results = requests.get(
        ENDPOINT,
        params={'query': TRIUMPHATOR_QUERY, 'format': 'application/json'}
    )
    # If the server is down, raise an exception and end.
    results.raise_for_status()

    results_json = results.json()
    keys = results_json['head']['vars']
    results_json = results_json['results']['bindings']

    for triumphator in results_json:
        triumphator_dict = {}
        for key in keys:
            triumphator_key = triumphator.get(key, {'value': None})
            triumphator_dict[key] = triumphator_key['value']
        try:
            # Look up the database ID for this DPRR person ID.
            about_person_id = triumphator_dict['personId']
            res = cur.execute('SELECT * FROM person WHERE dprr_id=?', (about_person_id,))
            about_person_db_id = None
            for row in res.fetchall():
                if row['dprr_id'] == int(about_person_id):
                    about_person_db_id = row['id']
            if about_person_db_id is None:
                print(f'Error looking up people for triumphator profile {triumphator}')
                continue
            # Determine if we need to insert the triumphator or if they already are in the database.
            res = cur.execute('SELECT * FROM triumphator WHERE about_person = ?', (about_person_db_id,))
            if len(res.fetchall()) == 0:
                cur.execute(
                    'INSERT INTO triumphator (about_person, source) VALUES (?, "DPRR")',
                    (about_person_db_id,)
                )
        except Exception as e:
            print(f'Error inserting triumphator {triumphator} into database: {e}')


# Query DPRR and update our database. Changes will not save if the script crashes.
conn = sqlite3.connect(DATABASE_LOCATION)
conn.row_factory = dict_factory
cur = conn.cursor()
setupDatabase(cur)
upsertPeople(cur)
upsertRelationships(cur)
upsertTriumphators(cur)
conn.commit()