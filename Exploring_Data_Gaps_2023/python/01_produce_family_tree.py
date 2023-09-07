"""
Queries the database and save family tree JSON files for each subtree.

Author: Benjamin Niedzielski (benjamin_niedzielski@alumni.brown.edu)
Last Modified: August 24, 2023
"""

# Imports
import json
import sqlite3

# Constants
DATABASE_PATH = 'roman_prosopography.db'
MAX_BIRTH_YEAR = -130

FATHER_RELATIONSHIP = 3
SPOUSE_RELATIONSHIP = 4
SON_RELATIONSHIP = 5
DAUGHTER_RELATIONSHIP = 8
DIVORCE_RELATIONSHIP = 11
MOTHER_RELATIONSHIP = 12
ICON_MAP = {
    'consul': 'Praetexta.png',
    'censor': 'Purpurea.png',
    'praetor': 'Praetexta.png',
    'triumphator': 'Picta.png'
}


def dict_factory(cursor, row):
    """Allows sqlite queries to return a dict.
    Unspecified behavior when joins result in multiple columns with the same name."""
    fields = [column[0] for column in cursor.description]
    return {key: value for key, value in zip(fields, row)}


def getAllPeople(cur):
    """
    Gets all people from the database.
    :param cur: The cursor to use to connect to the database.
    :returns: A dict of all people.
    """
    res = cur.execute('''
        SELECT
            p.*,
            CASE
                WHEN t.about_person IS NULL THEN 0
                ELSE 1
            END AS is_triumphator
        FROM person AS p
        LEFT JOIN triumphator AS t
            ON p.id=t.about_person
        WHERE p.birth <= ? OR p.birth IS NULL
        ''',
        (MAX_BIRTH_YEAR,)
    )
    return res.fetchall()


def getAllRelationships(cur):
    """
    Gets all relationships involving parents, spouses, and children from the database.
    :param cur: The cursor to use to connect to the database.
    :returns: A dict of all relationships.
    """
    res = cur.execute("""
    SELECT
        r.id,
        r.about_person,
        r.related_person,
        p.gender AS related_person_gender,
        r.type,
        rt.name
    FROM relationship AS r
    INNER JOIN relationship_type AS rt
        ON r.type = rt.id
    INNER JOIN person AS p
        ON r.related_person = p.id
    INNER JOIN person AS p2
        ON r.about_person = p2.id
    WHERE r.type IN (5, 8, 4, 11)  -- (Son, daughter, spouse, divorced)
        AND (p.birth <= ? OR p.birth IS NULL)
        AND (p2.birth <= ? OR p2.birth IS NULL)
    """, (MAX_BIRTH_YEAR, MAX_BIRTH_YEAR))
    return res.fetchall()


def populateRelationshipDicts(relationships):
    """
    Produces several lookups for relationships, including spouses and parents.
    :param relationships: A dict of relationships from the database, as returned by getAllRelationships().
    :returns: A tuple of lookups, in the order (spousal_relationships, spousal_keys, parental_relationship).
        spousal_relationships maps the ID of a person to the ID of their marriage node (person1_person2) to True.
        spousal_keys maps marriage node IDs (person1_person2) to True.
        parental_relationship maps the ID of a person to a dict with keys of father and mother and values of IDs or None
    """
    spousal_relationships = {}
    spousal_keys = {}
    parental_relationships = {}
    for relationship in relationships:
        # Operate entirely in strings to avoid type issues in Javascript.
        about_person = str(relationship['about_person'])
        related_person = str(relationship['related_person'])
        related_person_gender = relationship['related_person_gender']

        if relationship['type'] in [SON_RELATIONSHIP, DAUGHTER_RELATIONSHIP]:
            # Populate the mother or father within parental_relationships.
            if related_person_gender == 'Male':
                if about_person in parental_relationships:
                    parental_relationships[about_person]['father'] = related_person
                else:
                    parental_relationships[about_person] = {'father': related_person, 'mother': None}
            elif related_person_gender == 'Female':
                if about_person in parental_relationships:
                    parental_relationships[about_person]['mother'] = related_person
                else:
                    parental_relationships[about_person] = {'father': None, 'mother': related_person}
        elif relationship['type'] in [SPOUSE_RELATIONSHIP, DIVORCE_RELATIONSHIP]:
            # Create a relationship key, which will be used as a node ID for the marriage.
            # To avoid duplicates, the smaller ID comes first.
            # Note: When an unknown person is present, they always come last.
            if about_person < related_person:
                relationship_key = f'{about_person}_{related_person}'
            else:
                relationship_key = f'{related_person}_{about_person}'
            # Mark this marriage in spousal_keys and tie it to this person.
            # The spouse has their own relationship row and so will be handled in their own iteration.
            spousal_keys[relationship_key] = True
            if about_person in spousal_relationships:
                spousal_relationships[about_person][relationship_key] = True
            else:
                spousal_relationships[about_person] = {relationship_key: True}
    return spousal_relationships, spousal_keys, parental_relationships


def getPersonJSON(people, spousal_relationships):
    """
    Produces a JSON object for each person in people, for use in the JS Family Tree script.
    """
    person_json = {}
    for person in people:
        # Cast to a string to avoid type issues in Javascript.
        person_id = str(person['id'])

        if person.get('is_triumphator') == 1:
            best_pos = 'triumphator'
        else:
            best_pos = person.get('highest_office', '')
            # Parse the best position based on the DPRR string.
            if best_pos is None:
                best_pos = ''
            elif ("cos." in best_pos) and (best_pos.index("cos.") == 0):
                best_pos = "consul"
            elif ("cens." in best_pos) and best_pos.index("cens.") == 0:
                best_pos = "censor"
            elif ("pr." in best_pos) and best_pos.index("pr.") == 0:
                best_pos = "praetor"
            else:
                best_pos = ''

        person_json[person_id] = {
            'id': person_id,  # Represents the database ID, not the DPRR ID.
            'name': person['name'],
            'birthyear': person['birth'],
            'deathyear': person['death'],
            'dprr_id': person['dprr_id'],
            'own_unions': list(spousal_relationships.get(person_id, {}).keys()),  # Represents marriages.
            'is_certain': True if person['is_certain'] == 1 else False,
            'gender': person['gender'],
            'icon': ICON_MAP.get(best_pos, 'NoPosition.png' if person['gender'] == 'Male' else 'Woman.png')
        }
    return person_json


def insertPersonIntoDB(cur, person):
    """
    Given a person that we have inserted into the graph, add them to the database as well.
    :param cur: The cursor to the database.
    :param person: A JSON representation of the inserted person.
        Keys must include name, gender, and is_certain.
    :returns: The database id of the inserted person.
    """
    cur.execute(
        '''
        INSERT INTO person (name, nomen, cognomen, gender, highest_office, birth, death, dprr_id, source, is_certain)
        VALUES (?, NULL, NULL, ?, NULL, NULL, NULL, NULL, 'RomeLab', ?)
        ''',
        (person['name'], person['gender'], person['is_certain'])
    )
    return cur.lastrowid


def insertRelationshipIntoDB(cur, about_person, related_person, relationship_type):
    """
    Given the database ID of two people and a relationship type, creates a relationship between them.
    Note that both directions of the relationship need to be set up, so this function should be called twice.
    :param cur: The cursor to the database.
    :param about_person: The ID of the person this relationship is about.
    :param related_person: The ID of the person on the other side of this relationship.
    :param relationship_type: The ID of the type of relationship.
    :returns: The ID of the inserted row.
    """
    cur.execute(
        '''
        INSERT INTO relationship (about_person, related_person, type, dprr_id, source)
        VALUES (?, ?, ?, NULL, 'RomeLab')
        ''',
        (about_person, related_person, relationship_type)
    )
    return cur.lastrowid


def handleMissingParents(cur, parental_relationships, person_json, spousal_keys, spousal_relationships, people_genders):
    """
    Fills in gaps in data by adding missing spouses and connecting relationships directly to children.
    :param cur: The cursor connecting to the database, for any necessary insertions.
    :param parental_relationships: A dict mapping children to parents, as returned by populateRelationshipDicts.
    :param person_json: A dict of JSON objects for each person, as returned by getPersonJSON.
    :param spousal_keys: A dict with keys of all marriage relationship IDs, as returned by populateRelationshipDicts.
    :param spousal_relationships: A dict mapping people to relationship IDs, as returned by populateRelationshipDicts.
    :param people_genders: The genders of all people, so we can properly populate missing relationships.
    :return: A tuple consisting of (person_json, spousal_keys, spousal_relationships), updated to handle gaps.
        spousal_keys format is changed from the input to map relationshop ID to children IDs.
    """
    unknown_to_db = {}
    unknown_man_count = 1
    unknown_woman_count = 1
    for child_id in parental_relationships:
        child_id = str(child_id)
        parents = parental_relationships[child_id]
        father_id = str(parents['father']) if parents['father'] is not None else None
        mother_id = str(parents['mother']) if parents['mother'] is not None else None

        # Determine if a parent is missing.  If only 1 is missing, create a new person.  If both are missing,
        # do nothing.
        if father_id is None or father_id == '':
            # If the child's mother has a marriage already present, use that relationship for this child.
            if len(person_json[mother_id]['own_unions']) > 0:
                pair_ids = person_json[mother_id]['own_unions'][0].split('_')
                if mother_id == pair_ids[0]:
                    father_id = pair_ids[1]
                else:
                    father_id = pair_ids[0]
                insertRelationshipIntoDB(cur, unknown_to_db.get(father_id, father_id), child_id, FATHER_RELATIONSHIP)
                insertRelationshipIntoDB(
                    cur,
                    child_id,
                    unknown_to_db.get(father_id, father_id),
                    SON_RELATIONSHIP if people_genders[child_id] == 'Male' else DAUGHTER_RELATIONSHIP
                )
                relationship_key = f'{mother_id}_{father_id}'
            else:
                father_id = f'unknownman{unknown_man_count}'
                relationship_key = f'{mother_id}_{father_id}'
                person_json[father_id] = {
                    'id': father_id,
                    'name': 'Unknown Man',
                    'birthyear': None,
                    'deathyear': None,
                    'dprr_id': None,
                    'own_unions': [relationship_key],
                    'is_certain': False,
                    'gender': 'Male',
                    'icon': None
                }
                person_json[mother_id]['own_unions'].append(relationship_key)
                new_db_id = insertPersonIntoDB(cur, {'name': 'Unknown Man', 'gender': 'Male', 'is_certain': False})
                insertRelationshipIntoDB(cur, new_db_id, mother_id, SPOUSE_RELATIONSHIP)
                insertRelationshipIntoDB(cur, mother_id, new_db_id, SPOUSE_RELATIONSHIP)
                insertRelationshipIntoDB(cur, new_db_id, child_id, FATHER_RELATIONSHIP)
                insertRelationshipIntoDB(
                    cur,
                    child_id,
                    new_db_id,
                    SON_RELATIONSHIP if people_genders[child_id] == 'Male' else DAUGHTER_RELATIONSHIP
                )
                unknown_to_db[father_id] = new_db_id
                unknown_man_count = unknown_man_count + 1
                # Add the pairing to spousal_keys and spousal_relationships.
                spousal_keys[relationship_key] = True
                spousal_relationships[father_id] = {relationship_key: True}
                if mother_id in spousal_relationships:
                    spousal_relationships[mother_id][relationship_key] = True
                else:
                    spousal_relationships[mother_id] = {relationship_key: True}
        elif mother_id is None or mother_id == '':
            # If the child's father has a marriage already present, use that relationship for this child.
            if len(person_json[father_id]['own_unions']) > 0:
                pair_ids = person_json[father_id]['own_unions'][0].split('_')
                if father_id == pair_ids[0]:
                    mother_id = pair_ids[1]
                else:
                    mother_id = pair_ids[0]
                insertRelationshipIntoDB(cur, unknown_to_db.get(mother_id, mother_id), child_id, MOTHER_RELATIONSHIP)
                insertRelationshipIntoDB(
                    cur,
                    child_id,
                    unknown_to_db.get(mother_id, mother_id),
                    SON_RELATIONSHIP if people_genders[child_id] == 'Male' else DAUGHTER_RELATIONSHIP
                )
                relationship_key = f'{father_id}_{mother_id}'
            else:
                mother_id = f'unknownwoman{unknown_woman_count}'
                relationship_key = f'{father_id}_{mother_id}'
                person_json[mother_id] = {
                    'id': mother_id,
                    'name': 'Unknown Woman',
                    'birthyear': None,
                    'deathyear': None,
                    'dprr_id': None,
                    'own_unions': [relationship_key],
                    'is_certain': False,
                    'gender': 'Female',
                    'icon': None
                }
                person_json[father_id]['own_unions'].append(relationship_key)
                new_db_id = insertPersonIntoDB(cur, {'name': 'Unknown Woman', 'gender': 'Female', 'is_certain': False})
                insertRelationshipIntoDB(cur, new_db_id, father_id, SPOUSE_RELATIONSHIP)
                insertRelationshipIntoDB(cur, father_id, new_db_id, SPOUSE_RELATIONSHIP)
                insertRelationshipIntoDB(cur, new_db_id, child_id, MOTHER_RELATIONSHIP)
                insertRelationshipIntoDB(
                    cur,
                    child_id,
                    new_db_id,
                    SON_RELATIONSHIP if people_genders[child_id] == 'Male' else DAUGHTER_RELATIONSHIP
                )
                unknown_to_db[mother_id] = new_db_id
                unknown_woman_count = unknown_woman_count + 1
                spousal_keys[relationship_key] = True
                spousal_relationships[mother_id] = {relationship_key: True}
                if father_id in spousal_relationships:
                    spousal_relationships[father_id][relationship_key] = True
                else:
                    spousal_relationships[father_id] = {relationship_key: True}
        else:
            if father_id < mother_id:
                relationship_key = f'{father_id}_{mother_id}'
            else:
                relationship_key = f'{mother_id}_{father_id}'
        # Ensure that there are no missing connections remaining.
        # Additionally, change spousal_keys so that it maps each relationship id to children IDs to True.
        # Each relationship must know its children to populate JSON nodes.
        if not relationship_key in spousal_keys or spousal_keys[relationship_key] is True:
            spousal_keys[relationship_key] = {child_id: True}
        else:
            spousal_keys[relationship_key][child_id] = True
        if father_id not in spousal_relationships:
            spousal_relationships[father_id] = {relationship_key: True}
        elif relationship_key not in spousal_relationships[father_id]:
            spousal_relationships[father_id][relationship_key] = True
        if mother_id not in spousal_relationships:
            spousal_relationships[mother_id] = {relationship_key: True}
        elif relationship_key not in spousal_relationships[mother_id]:
            spousal_relationships[mother_id][relationship_key] = True
    return person_json, spousal_keys, spousal_relationships


def getLinkAndUnionJSON(spousal_keys):
    """
    Generates JSON for each link and union (marriage).
    :param spousal_keys: A dict mapping relationship id to children to True.
    :returns: A tuple of (unions_json, links_json).
        unions_json represents each marriage and knows the partners and children.
        links_json represents each directed edge in the graph, connecting spouses to unions and unions to children.
    """
    unions_json = {}
    links_json = []
    for union in spousal_keys:
        spouses = union.split('_')
        children = spousal_keys[union]
        if children is not True:
            children = [str(x) for x in list(children.keys())]
        else:
            children = []
        unions_json[union] = {
            'id': union,
            'partner': spouses,
            'children': children
        }
        for spouse in spouses:
            links_json.append([str(spouse), str(union)])
        for child in children:
            links_json.append([str(union), str(child)])
    return unions_json, links_json


def generateSingleGraph(target_person, links_json, person_json, unions_json):
    """
    Generates a JS file for a single family tree graph.
    :param target_person: The ID number of the person whose graph we should generate.
    :param links_json: The JSON for every edge in the full graph.
    :param person_json: The JSON for every person node in the full graph.
    :param unions_json: The JSON for every marriage node in the full graph.
    :returns: A list of all people in this graph.
    """
    target_person = str(target_person)
    # Iteratively expand the graph outwards from the target until no new nodes are added.
    # This represents the full graph.
    related_to_target = {target_person: True}
    target_unions = {}
    continue_search = True
    while continue_search:
        new_unions = {}
        new_people = {}
        # Links connect two people; either side may be connected to our graph.
        # If both or neither are connected, no action is needed.
        for link in links_json:
            first_link_connected = link[0] in related_to_target or link[0] in target_unions
            second_link_connected = link[1] in related_to_target or link[1] in target_unions
            # Determine if the addition to the graph is a marriage node or a person node.
            if first_link_connected and not second_link_connected:
                if '_' in link[1]:
                    new_unions[link[1]] = True
                else:
                    new_people[link[1]] = True
            elif second_link_connected and not first_link_connected:
                if '_' in link[0]:
                    new_unions[link[0]] = True
                else:
                    new_people[link[0]] = True
        # If there are no new connections, we are done iterating.
        if len(new_unions) == 0 and len(new_people) == 0:
            continue_search = False
        else:
            for union in new_unions:
                target_unions[union] = True
            for person in new_people:
                related_to_target[person] = True

    # Create the JSON for this subgraph
    new_person_json = {}
    new_unions_json = {}
    new_links_json = []
    for person in person_json:
        if person in related_to_target:
            new_person_json[person] = person_json[person]
    for union in unions_json:
        if union in target_unions:
            new_unions_json[union] = unions_json[union]
    for link in links_json:
        if (
            link[0] in related_to_target or
            link[1] in related_to_target or
            link[0] in target_unions or
            link[1] in target_unions
        ):
            new_links_json.append(link)

    # Generate the js file for this graph.
    output_json = {
        'start': target_person,
        'persons': new_person_json,
        'unions': new_unions_json,
        'links': new_links_json
    }

    f = open(f'../data/roman_data_{target_person}.js', 'w')
    f.write(f'data = {json.dumps(output_json)}')
    f.close()
    return list(related_to_target.keys())


def generateUniqueGraphs(links_json, person_json, unions_json):
    """
    Produces .js files for each unique subgraph.
    :param links_json: The JSON for every edge in the full graph.
    :param person_json: The JSON for every person node in the full graph.
    :param unions_json: The JSON for every marriage node in the full graph.
    """
    # Retrieve a dict of all people so we can generate all unique graphs.
    remaining_people = {}
    # Create a mapping from person to the file name of the graph storing their information.
    all_people = {}
    people_autocomplete = []
    for person_id in person_json:
        person = person_json[person_id]
        remaining_people[person['id']] = True
        if not 'Unknown' in person['name']:
            all_people[person_id] = {'name': person['name']}
            people_autocomplete.append({
                'value': person['name'],
                'id': person_id,
                'label': person['name']
            })

    while len(remaining_people) > 0:
        remaining_people_list = list(remaining_people.keys())
        target_person = remaining_people_list[0]
        people_in_graph = generateSingleGraph(target_person, links_json, person_json, unions_json)
        for person in people_in_graph:
            if not 'Unknown' in person_json[person]['name']:
                all_people[person]['filename'] = f'data/roman_data_{target_person}.js'
            del remaining_people[person]
    f = open(f'../data/all_people.js', 'w')
    f.write(f'people_mapping = {json.dumps(all_people)};')
    f.write("\n")
    f.write(f'peopleJSON = {json.dumps(people_autocomplete)};')
    f.close()


conn = sqlite3.connect(DATABASE_PATH)
conn.row_factory = dict_factory
cur = conn.cursor()
people = getAllPeople(cur)
# Get a lookup of all genders to add missing relationships to the database.
people_genders = {}
for person in people:
    people_genders[str(person['id'])] = person['gender']
relationships = getAllRelationships(cur)

(spousal_relationships, spousal_keys, parental_relationships) = populateRelationshipDicts(relationships)
# Generate the JSON for all nodes.  We will filter this into distinct trees later.
person_json = getPersonJSON(people, spousal_relationships)
# This updates spousal_relationships in case we expand this project. The result is not currently used.
(person_json, spousal_keys, spousal_relationships) = handleMissingParents(
    cur,
    parental_relationships,
    person_json,
    spousal_keys,
    spousal_relationships,
    people_genders
)
conn.commit()
(unions_json, links_json) = getLinkAndUnionJSON(spousal_keys)
generateUniqueGraphs(links_json, person_json, unions_json)
