"""
Generates JSON data for Roman funeral visualizations.

This program accepts a CSV fils containing information about Romans
and uses them to generate a list of Roman funeral data (sorted by death date)
including birth/death dates and the type of toga they would be represented by.

Also generates an autocomplete JSON data block for individuals
and data for gens search.

Author: Benjamin Niedzielski (benjamin_niedzielski@alumni.brown.edu)
Last Modified: 7/1/2022
"""

# Imports
import json

# Custom Settings
START_YEAR = -600  # The first death year to include in the visualization
FINAL_YEAR = 100   # The last year to include in the visualization

# Globals
all_people = {}
all_deaths = []  # Used to sort by death date
nomen_dict = {}

# Constants and enums
COMBINED_DATA_PATH = '../json/combined_data.json'
WRITE_PATH = '../js/database_data.js'
ICON_MAP = {
    'consul': 'Praetexta.png',
    'censor': 'Purpurea.png',
    'praetor': 'Praetexta.png',
    'triumphator': 'Picta.png'
}
CONSERVATIVE = 0
AGGRESSIVE = 1
HYPERAGGRESSIVE = 2


# Class definitions
class Nomen():
    """Contains the name of the nomen and a list of associated people."""

    def __init__(self, nomen):
        """
        Create a new nomen.

        :param nomen: The nomen this represents.
        """
        self.name = nomen
        self.people = []

    def __eq__(self, other):
        """
        Test equality based on the name.

        :param other: The nomen to compare to.
        :return: True if the names are the same, or False otherwise.
        """
        if other is None:
            return False
        return self.name == other.name

    def __ne__(self, other):
        """
        Test inequality based on the name.

        :param other: The nomen to compare to.
        :return: True if the names are not the same, or False otherwise.
        """
        if other is None:
            return True
        return self.name != other.name


# Each person has relevant information, such as name, birthdate, and relatives
class Person():
    """Contains person information, such as name, birthdate, and relatives."""

    def __init__(self, person_json):
        """
        Create a person from JSON data about the person.

        :param person_json: A JSON object with person information.
        """
        self.name = person_json.get('name', '')
        self.nomen = person_json.get('nomen', '')
        self.cognomen = person_json.get('cognomen', '')
        self.id = int(person_json.get('id', 0))
        self.birthDate = int(person_json.get('birth', 0))
        self.deathDate = int(person_json.get('death', 0))
        self.relatives = {}
        best_pos = person_json.get('highestOffice', '')

        # Parse the best position based on the DPRR string.
        if best_pos is None:
            best_pos = ''
        if person_json.get('triumphator', None) is not None:
            self.bestPosition = 'triumphator'
        elif ("cos." in best_pos) and (best_pos.index("cos.") == 0):
            self.bestPosition = "consul"
        elif ("cens." in best_pos) and best_pos.index("cens.") == 0:
            self.bestPosition = "censor"
        elif ("pr." in best_pos) and best_pos.index("pr.") == 0:
            self.bestPosition = "praetor"
        else:
            self.bestPosition = ''

        fatherID = person_json.get('fatherID', None)
        father_uncertain = person_json.get('fatherIsUncertain', False)
        if fatherID is not None:
            self.fatherID = int(fatherID)
            self.relatives[int(fatherID)] = {
                'type': 'father',
                'certainty': False if father_uncertain else True,
                'person': None  # Will link to Person object after construction
            }
        else:
            self.fatherID = None

        grandfatherID = person_json.get('grandfatherID', None)
        if grandfatherID == "None":
            grandfatherID = None
        grandfather_uncertain = person_json.get(
            'grandfatherIsUncertain',
            False
        )
        if grandfatherID is not None:
            self.grandfatherID = int(grandfatherID)
            self.relatives[int(grandfatherID)] = {
                'type': 'grandfather',
                'certainty': False if grandfather_uncertain else True,
                'person': None  # Will link to Person object after construction
            }
        else:
            self.grandfatherID = None

    def getFuneralJSON(self):
        """
        Get information about the person in JSON format for funeral dumps.

        :return: A JSON object with information about the person.
        """
        return {
            'name': self.name,
            'id': self.id,
            'nomen': self.nomen,
            'bestPos': self.bestPosition,
            'birthDate': self.birthDate,
            'deathDate': self.deathDate,
            'icon': ICON_MAP.get(self.bestPosition, 'NoPosition.png')
        }

    def getLookupJSON(self):
        """
        Get information about the person in JSON format for JS autocomplete.

        :return: A JSON object with information about the person.
        """
        return {
            'value': self.name + ', d. ' + str(self.deathDate),
            'id': self.id
        }


def parseCombinedDataFile():
    """Populate global people data based on a JSON file."""
    with open(COMBINED_DATA_PATH, 'r', encoding='utf-8') as f:
        data_json = json.load(f)
    for person_json in data_json:
        person = Person(person_json)
        all_deaths.append((person.id, person.deathDate))
        all_people[person.id] = person

        # Set up the nomen dictionary to include this person for fast lookup.
        if (nomen_dict.get(person.nomen, None) is not None):
            nomen_dict[person.nomen].people.append(person)
        else:
            nomen = Nomen(person.nomen)
            nomen.people.append(person)
            nomen_dict[person.nomen] = nomen


def buildNodes():
    """Set up the list of relatives to allow easier access for recursion."""
    for person_id in all_people:
        person = all_people[person_id]
        relatives_to_delete = []
        if person is not None:
            for relative_id in person.relatives:
                # Remove any connections not present in the data.
                if all_people.get(relative_id) is None:
                    relatives_to_delete.append(relative_id)
            for relative_id in relatives_to_delete:
                del person.relatives[relative_id]
            for relative_id in person.relatives:
                person.relatives[relative_id]['person'] =  \
                    all_people[relative_id]
    for person_id in all_people:
        person = all_people[person_id]


def recursiveRoles(mode, index, people_so_far):
    """
    Recusrively build data about a person and all their paternal ancestors.

    Different models will fill gaps in different ways:
    CONSERVATIVE: Do not fill gaps.
    AGGRESSIVE: Fill gaps based on nomen, cognomen, and birth year.
    HYPERAGGRESSIVE: Fill gaps based on nomen and birth year.

    :param mode: The model, CONSERVATIVE or AGGRESSIVE or HYPERAGGRESSIVE
    :param index: The ID of the person to return info on
    :param people_so_far: A list of people already considered in this query
    :return: JSON representing the person and their ancestors.
    """
    # Avoid infinite loops caused by bad data
    if index in people_so_far:
        print(f'Reached an infinite loop around person with index {index}.')
        return []

    if all_people.get(index, None) is not None:
        person = all_people[index]
        people_so_far.append(index)

        # If a father is attested, it should be used in all cases
        if (
            person.fatherID is not None and
            (
                mode == CONSERVATIVE or
                person.relatives[person.fatherID]['certainty'] or
                person.grandfatherID is None
                or not person.relatives[person.grandfatherID]['certainty']
            )
        ):
            # Do not include people without political positions
            # unless it is their funeral: len(people_so_far) == 1
            if person.bestPosition == '' and len(people_so_far) != 1:
                return recursiveRoles(mode, person.fatherID, people_so_far)
            else:
                return [person.getFuneralJSON()] + recursiveRoles(
                    mode,
                    person.fatherID,
                    people_so_far
                )
        else:
            # Check if grandfather is attested, it should be used in all cases
            if person.grandfatherID is not None:
                if person.bestPosition == '' and len(people_so_far) != 1:
                    return recursiveRoles(
                        mode,
                        person.grandfatherID,
                        people_so_far
                    )
                else:
                    return [person.getFuneralJSON()] + recursiveRoles(
                        mode,
                        person.grandfatherID,
                        people_so_far
                    )

            # If none is attested, the conservative approach would end.
            # Special cases for people whose data is impossible
            # (older than their father)
            # TODO: Make these exceptions more general,
            # since they can be overridden manually.
            if (
                mode == CONSERVATIVE or index in [1095, 1032, 1235, 4695]
            ):
                if person.bestPosition == '' and len(people_so_far) != 1:
                    return []
                else:
                    return [person.getFuneralJSON()]
            else:
                # Search the nomen dictionary.  Find the most likely father
                # candidate (someone 22+ years older in the same Gens, ideally
                # with the same cognomen)
                personNomen = person.nomen
                if personNomen != "":
                    nomen = nomen_dict.get(personNomen)
                    sameCognomen = []
                    sameNomen = []
                    for x in nomen.people:
                        # Special cases for people whose data is impossible.
                        # TODO: Build dynamically in case data is edited.
                        if (
                            x.cognomen == person.cognomen and
                            x.birthDate < person.birthDate - 22 and
                            (index != 3264 or x.id != 3957) and
                            (index != 3264 or x.id != 3956) and
                            (index != 4453 or x.id != 5311) and
                            (index != 5297 or x.id != 4116) and
                            (index != 5297 or x.id != 4118) and
                            (index != 3788 or x.id != 3573) and
                            (index != 3349 or x.id != 624) and
                            (index != 1235 or x.id != 1032) and
                            (index != 4026 or x.id != 4093)
                        ):
                            sameCognomen.append(x)
                        if (
                            x.birthDate < person.birthDate - 22 and
                            (index != 3264 or x.id != 3957) and
                            (index != 3264 or x.id != 3956) and
                            (index != 4453 or x.id != 5311) and
                            (index != 5297 or x.id != 4116) and
                            (index != 5297 or x.id != 4118) and
                            (index != 3788 or x.id != 3573) and
                            (index != 3349 or x.id != 580) and
                            (index != 3349 or x.id != 624) and
                            (index != 1235 or x.id != 1032) and
                            (index != 4026 or x.id != 4093)
                        ):
                            sameNomen.append(x)
                    bestFitId = 0
                    bestFitBirth = -600
                    for x in sameCognomen:
                        if x.birthDate > bestFitBirth:
                            bestFitBirth = x.birthDate
                            bestFitId = x.id
                    if len(sameCognomen) == 0 or bestFitId == 0:
                        bestFitId = 0
                        bestFitBirth = -600
                        for x in sameNomen:
                            if x.birthDate > bestFitBirth:
                                bestFitBirth = x.birthDate
                                bestFitId = x.id
                        if (
                            len(sameNomen) == 0 or
                            bestFitId == 0 or
                            not(mode == HYPERAGGRESSIVE)
                        ):
                            if (
                                person.bestPosition == '' and
                                len(people_so_far) != 1
                            ):
                                return []
                            else:
                                return [person.getFuneralJSON()]
                        else:
                            if (
                                person.bestPosition == '' and
                                len(people_so_far) != 1
                            ):
                                return recursiveRoles(
                                    mode,
                                    bestFitId,
                                    people_so_far
                                )
                            else:
                                return [person.getFuneralJSON()] +  \
                                    recursiveRoles(
                                        mode,
                                        bestFitId,
                                        people_so_far
                                    )
                    else:
                        if (
                            person.bestPosition == '' and
                            len(people_so_far) != 1
                        ):
                            return recursiveRoles(
                                mode,
                                bestFitId,
                                people_so_far
                            )
                        else:
                            return [person.getFuneralJSON()] + recursiveRoles(
                                mode,
                                bestFitId,
                                people_so_far
                            )
    return []


def sortDates():
    """Sort people by death date."""
    for indexOne in range(0, len(all_deaths) - 1):
        for indexTwo in range(indexOne + 1, len(all_deaths)):
            if all_deaths[indexOne][1] > all_deaths[indexTwo][1]:
                temp = all_deaths[indexOne]
                all_deaths[indexOne] = all_deaths[indexTwo]
                all_deaths[indexTwo] = temp


def traverseNodes(mode):
    """
    Produce the lineage history for all people, sorted by death date.

    :param mode: The model, CONSERVATIVE or AGGRESSIVE or HYPERAGGRESSIVE
    :return: A JSON object representing each person's funeral lineage data.
    """
    sortDates()
    all_funerals = []
    # Create blocks of 25 year periods.
    current_period_funerals = []
    current_period_start = -600
    # Adding 24 gives a 25 year range.
    current_period_end = current_period_start + 24
    for death in all_deaths:
        person_id = death[0]
        death_year = death[1]
        if death_year < START_YEAR:
            continue
        if death_year > FINAL_YEAR:
            break
        # Advance to the next block of time.
        while death_year > current_period_start + 24:
            if len(current_period_funerals) > 0:
                current_period_funerals = sortFuneralsBySize(
                    current_period_funerals
                )
                all_funerals.append({
                    'timePeriod': f'{abs(current_period_start)}' +
                    f'–{abs(current_period_end)} ' +
                    f'{"CE" if current_period_end >= 0 else "BCE"}',
                    'funerals': current_period_funerals
                })
            current_period_funerals = []
            current_period_start += 25
            current_period_end += 25
        person_funeral_json = recursiveRoles(mode, person_id, [])
        current_period_funerals.append(person_funeral_json)
    # Include the final block of funerals.
    if len(current_period_funerals) > 0:
        current_period_funerals = sortFuneralsBySize(current_period_funerals)
        all_funerals.append({
            'timePeriod': f'{abs(current_period_start)}' +
            f'–{abs(current_period_end)} ' +
            f'{"CE" if current_period_end >= 0 else "BCE"}',
            'funerals': current_period_funerals
        })
    return {'timePeriods': all_funerals}


def sortFuneralsBySize(funerals):
    """
    Sort the funerals in a time block from most ancestors to fewest.

    :param funerals: A list of funerals to order by length.
    :return: The list of funerals ordered by number of ancestors.
    """
    for ii in range(0, len(funerals) - 1):
        for jj in range(ii + 1, len(funerals)):
            if len(funerals[ii]) < len(funerals[jj]):
                temp = funerals[ii]
                funerals[ii] = funerals[jj]
                funerals[jj] = temp
    return funerals


def buildDropdownLookup():
    """
    Create a JSON object representing all people for JS Autocomplete.

    :return: A JSON object containing all people.
    """
    lookup_json = []
    for person_id in all_people:
        person = all_people[person_id]
        lookup_json.append(person.getLookupJSON())
    return lookup_json


def buildGensLookup():
    """
    Build a JSON object representing each gens for an HTML select.

    :return: A JSON object representing each gens.
    """
    gens_list = [nomen.name for nomen in nomen_dict.values()]
    gens_list.sort()
    gens_json = []
    for nomen in gens_list:
        gens_json.append({'value': nomen, 'id': nomen})
    return gens_json


def buildFuneralDataFile(write_file):
    """
    Write a JS file containing funeral data for visualizations.

    :param write_file: The path of the file to write.
    """
    with open(write_file, 'w', encoding='utf-8') as f:
        f.write("// File automatically generated by Python script.\n\n")
        f.write("const conservativeFuneralData =\n")
        data = traverseNodes(CONSERVATIVE)
        json.dump(data, f, ensure_ascii=False, indent=4)
        f.write(";\n\nconst aggressiveFuneralData =\n")
        data = traverseNodes(AGGRESSIVE)
        json.dump(data, f, ensure_ascii=False, indent=4)
        f.write(";\n\nconst hyperaggressiveFuneralData =\n")
        data = traverseNodes(HYPERAGGRESSIVE)
        json.dump(data, f, ensure_ascii=False, indent=4)
        f.write(";\n\nconst autocompleteJSON =\n")
        lookup = buildDropdownLookup()
        json.dump(lookup, f, ensure_ascii=False, indent=4)
        f.write(";\n\nconst gensJSON =\n")
        lookup = buildGensLookup()
        json.dump(lookup, f, ensure_ascii=False, indent=4)
        f.write(";\n")


# Build the people objects, then run the program.
parseCombinedDataFile()
buildNodes()
buildFuneralDataFile(WRITE_PATH)
