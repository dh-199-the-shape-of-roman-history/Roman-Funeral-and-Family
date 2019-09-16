############################################################################
#This program accepts various CSV files containing information about Romans
#and uses them to generate a list of Romans (sorted by death date) and
#the highest rank they and each of their ancestors achieved.
#Users are prompted to choose between a conservative or aggressive algorithm
#and whether they want verbose or concise output.
#The Ruby file associated with this project assumes the concise output.
#
#Author: Benjamin Niedzielski (bniedzie@ucla.edu)
#Last Modified: 9/11/2018
############################################################################

#Globals
SIZE = 6000;                                #The array size
allPeople = [None] * SIZE;
allDeaths = [];                             #Used to sort by death date
nameIDFile = "NameOfficeDeathQuery.csv";    #Contains rows of Roman males, with format of ID, Name, Best office, Death date
triumphFile = "triumphList.csv";            #Contains rows of those who triumphed, with format of ID, Name
fatherFile = "FatherQuery.csv";             #Contains rows of paternal relations, with format of Son ID, Father ID
nomenFile = "NomenList.csv";                #Contains rows of Roman males, with the format of ID, , Gens, Cognomen (sic)
birthFile = "birthQuery.csv";               #Contains rows of Roman males, with the format of ID, Birth date
nomenDictionary = {};

#A Nomen consists of the name of that nomen and a list of people with that nomen.
class Nomen():
    def __init__(self):
        self.name = "";
        self.people = [];
    #Nomen equality is based on the name
    def __eq__(self, other):
        if (other == None):
            return False;
        return self.name == other.name;
    def __ne__(self, other):
        if (other == None):
            return True;
        return self.name != other.name;

#Each person has relevant information, such as name, birthdate, and relatives
class Person():
    def __init__(self):
        self.name = "";
        self.nomen = "";
        self.cognomen = "";
        self.id = 0;
        self.birthDate = 0;
        self.deathDate = 0;
        self.relativeIDs = [];
        self.relativeType = [];
        self.relatives = [];
        self.bestPosition = "";

#Parse the file containing names, ids, and best position attained
def parseNameIDFile():
    with open(nameIDFile) as f:
        content = f.readlines();
    content = [x.strip() for x in content];
    for line in content:
        splitLine = line.split(",");
        person = Person();
        person.id = int(splitLine[0]);
        
        person.name = splitLine[1];
        if ("cos." in splitLine[2]) and (splitLine[2].index("cos.") == 0):
            person.bestPosition = "consul";
        elif ("cens." in splitLine[2]) and splitLine[2].index("cens.") == 0:
            person.bestPosition = "censor";
        elif ("pr." in splitLine[2]) and splitLine[2].index("pr.") == 0:
            person.bestPosition = "praetor";
        person.deathDate = int(splitLine[3]);
        allDeaths.append([person.id, person.deathDate]);
        allPeople[person.id] = person;

#Parse the file containing the birth date of each person
def parseBirthFile():
    with open(birthFile) as f:
        content = f.readlines();
    content = [x.strip() for x in content];
    for line in content:
        splitLine = line.split(",");
        id = int(splitLine[0]);
        if (allPeople[id] != None):
            allPeople[id].birthDate = int(splitLine[1]);

#Parse the file containing the nomen and cognomen of each person
def parseNomenFile():
    with open(nomenFile) as f:
        content = f.readlines();
    content = [x.strip() for x in content];
    for line in content:
        splitLine = line.split(",");
        id = int(splitLine[0]);
        if (allPeople[id] != None):
            allPeople[id].nomen = splitLine[2];
            allPeople[id].cognomen = splitLine[3];
            #set up the nomen dictionary to include this person for fast lookup later
            if (nomenDictionary.get(splitLine[2]) != None):
                nomenDictionary[splitLine[2]].people.append(allPeople[id]);
            else:
                nomen = Nomen();
                nomen.name = splitLine[2];
                nomen.people.append(allPeople[id]);
                nomenDictionary[splitLine[2]] = nomen;

#Parse the file containing which people triumphed
def parseTriumphFile():
    with open(triumphFile) as f:
        content = f.readlines();
    content = [x.strip() for x in content];
    for line in content:
        splitLine = line.split(",");
        id = int(splitLine[0]);
        if (allPeople[id] != None):
            allPeople[id].bestPosition = "triumphator";

#Parse the file containing the father of each person
def parseFatherFile():
    with open(fatherFile) as f:
        content = f.readlines();
    content = [x.strip() for x in content];
    for line in content:
        splitLine = line.split(",");
        id = int(splitLine[0]);
        fatherID = int(splitLine[1]);
        if (allPeople[id] != None):
            #Allows support for other types of relationships as well
            allPeople[id].relativeIDs.append(fatherID);
            allPeople[id].relativeType.append("father");

#Parse all relevant files to set up the people needed
def parseInput():
    parseNameIDFile();
    parseNomenFile();
    parseBirthFile();
    parseTriumphFile();
    parseFatherFile();
    
#Sets up the list of relatives to allow easier access for recursion
def buildNodes():
    for person in allPeople:
        if person != None:
            for id in person.relativeIDs:
                person.relatives.append(allPeople[id]);
    return;

#Recusrively build up an output giving the highest positions by a person and all their paternal ancestors
#
#Input: conservative - a boolean, true if the output should be limited to directly attested fathers, false if it should guess if none is given
#Input: verbose - a boolean, true if the output should give details like ancestor names
#input: index - the id of the person to return info on
#Output: A string containing the highest position of this person and their ancestors, formatted nicely
def recursiveRoles(conservative, verbose, index):
    if allPeople[index] != None:
        person = allPeople[index];
        #If a father is attested, it should be used in all cases
        if "father" in person.relativeType:
            if verbose:
                return "\t" + person.name + " " + person.bestPosition + "\n" + recursiveRoles(conservative, verbose, person.relativeIDs[person.relativeType.index("father")]);
            else:
                return person.bestPosition + " " + recursiveRoles(conservative, verbose, person.relativeIDs[person.relativeType.index("father")]);   
        else:
            #If none is attested, the conservative approach would end.  Special cases for people whose data is impossible (older than their father)
            if (conservative or index == 1095 or index == 1032 or index == 1235 or index == 4695):
                if verbose:
                    return "\t" + person.name + " " + person.bestPosition;
                else:
                    return person.bestPosition;
            else:
                #Search the nomen dictionary.  Find the most likely father candidate (someone 22+ years older in the same Gens, ideally with the same cognomen)
                personNomen = person.nomen;
                if (personNomen != ""):
                    nomen = nomenDictionary.get(personNomen);
                    sameCognomen = [];
                    sameNomen = [];
                    for x in nomen.people:
                        #Special cases for people whose data is impossible, as above
                        if x.cognomen == person.cognomen and x.birthDate < person.birthDate - 22 and (index != 3349 or x.id != 624) and (index != 1235 or x.id != 1032) and (index != 4026 or x.id != 4093):
                            sameCognomen.append(x);
                        if x.birthDate < person.birthDate - 22 and (index != 3349 or x.id != 624) and (index != 1235 or x.id != 1032) and (index != 4026 or x.id != 4093):
                            sameNomen.append(x);
                    if (len(sameCognomen) == 0):
                        if (len(sameNomen) == 0):
                            if verbose:
                                return "\t" + person.name + " " + person.bestPosition;
                            else:
                                return person.bestPosition;
                        bestFitId = 0;
                        bestFitBirth = -600;
                        for x in sameNomen:
                            if x.birthDate > bestFitBirth:
                                bestFitBirth = x.birthDate;
                                bestFitId = x.id;
                        if verbose:
                            return "\t" + person.name + " " + person.bestPosition + "\n\t" + "(Nomen guess)" + recursiveRoles(conservative, verbose, bestFitId);
                        else:
                            return person.bestPosition + " " + recursiveRoles(conservative, verbose, bestFitId);
                    bestFitId = 0;
                    bestFitBirth = -600;
                    for x in sameCognomen:
                        if x.birthDate > bestFitBirth:
                            bestFitBirth = x.birthDate;
                            bestFitId = x.id;
                    if verbose:
                        return "\t" + person.name + " " + person.bestPosition + "\n\t" + "(Cognomen guess)" + recursiveRoles(conservative, verbose, bestFitId);
                    else:
                        return person.bestPosition + " " + recursiveRoles(conservative, verbose, bestFitId);
    return "";

#Sorts people by death date
def sortDates():
    for indexOne in range(0, len(allDeaths) - 1):
         for indexTwo in range(indexOne + 1, len(allDeaths)):
             if (allDeaths[indexOne][1] > allDeaths[indexTwo][1]):
                 temp = allDeaths[indexOne];
                 allDeaths[indexOne] = allDeaths[indexTwo];
                 allDeaths[indexTwo] = temp;

#Prints the lineage history for all people, sorted by death date
#
#Input: conservative - a boolean, true if the output should be limited to directly attested fathers, false if it should guess if none is given
#Input: verbose - a boolean, true if the output should give details like ancestor names
def traverseNodes(conservative, verbose):
    sortDates();
    for death in allDeaths:
        #print(death[1]);
        person = allPeople[death[0]];
        print (person.name + " " + str(person.deathDate));
        #print (person.name);
        parentRoles = recursiveRoles(conservative, verbose, person.id);
        print (parentRoles + "\n");

#Prints the lineage history for a single person
#
#Input: conservative - a boolean, true if the output should be limited to directly attested fathers, false if it should guess if none is given
#Input: verbose - a boolean, true if the output should give details like ancestor names
#input: index - the id of the person to return info on
def traversePerson(conservative, verbose, id):
    person = allPeople[id];
    if person != None:
        #print (person.name);
        parentRoles = recursiveRoles(conservative, verbose, person.id);
        print (parentRoles);

#Build the people objects, then run the program
parseInput();
buildNodes();
while (True):
    print("Use the conservative approach? y/n")
    query1 = input().strip().lower();
    if (query1 == 'y' or query1 == 'n'):
        break;
while (True):
    print("Produce verbose output? y/n")
    query2 = input().strip().lower();
    if (query2 == 'y' or query2 == 'n'):
        break;
#Leave the following line uncommented to output every male in the database
traverseNodes(query1 == 'y', query2 == 'y');
#Uncomment the following 3 lines to give output for a single male
#print("Enter an ID to query.");
#query3 = input();
#traversePerson(query1 == 'y', query2 == 'y', int(query3));
