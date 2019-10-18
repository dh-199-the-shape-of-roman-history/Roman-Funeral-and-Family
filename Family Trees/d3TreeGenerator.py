############################################################################
#This program accepts various CSV files containing information about Romans
#and uses them to generate a family tree in d3.js.
#Produces different trees representing different levels of certainty.
#
#Author: Benjamin Niedzielski (bniedzie@ucla.edu)
#Last Modified: 5/20/2018
############################################################################
import networkx as nx;
import matplotlib.pyplot as plt;

#Globals
SIZE = 6000;                                        #The array size
allPeople = [None] * SIZE;
allDeaths = [];                                     #Used to sort by death date
nameIDFile = "NameOfficeDeathQuery.csv";            #Contains rows of Roman males, with format of ID, Name, Best office, Death date
triumphFile = "triumphList.csv";                    #Contains rows of those who triumphed, with format of ID, Name
fatherFile = "FatherQuery.csv";                     #Contains rows of paternal relations, with format of Son ID, Father ID
cognomenFile = "NomenList.csv";                     #Contains rows of Roman males, with the format of ID, , Gens, Cognomen (sic)
nomenFile = "NomenQuery.csv";                       #Contains rows of Roman males, with the format of ID, Gens
birthFile = "birthQuery.csv";                       #Contains rows of Roman males, with the format of ID, Birth date
grandfatherFile = "grandfatherQuery.csv";           #Contains rows of relations, with format of Grandson ID, Grandfather ID
fatherUCFile = "fatherUncertainQuery.csv";          #Contains rows of relations where the father is uncertain, with format of Son ID, Father ID
grandfatherUCFile = "grandfatherUncertainQuery.csv";#Contains rows of relations where the grandfather is uncertain, with format of Grandson ID, Grandfather ID
nomenDictionary = {};
positions = [];
pplNames = {};
edgeColors = [];
pplAdded = set();

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
        self.relativeCertainty = [];
        self.relatives = [];
        self.bestPosition = "";
        self.gensID = -1;

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

#Parse the file containing the nomen of each person
def parseNomenFile():
    with open(nomenFile) as f:
        content = f.readlines();
    content = [x.strip() for x in content];
    for line in content:
        splitLine = line.split(",");
        id = int(splitLine[0]);
        if (allPeople[id] != None):
            allPeople[id].nomen = splitLine[1];
            #set up the nomen dictionary to include this person for fast lookup later
            if (nomenDictionary.get(splitLine[1]) != None):
                nomenDictionary[splitLine[1]].people.append(allPeople[id]);
            else:
                nomen = Nomen();
                nomen.name = splitLine[1];
                nomen.people.append(allPeople[id]);
                nomenDictionary[splitLine[1]] = nomen;

#Parse the file containing the cognomen of each person
def parseCognomenFile():
    with open(cognomenFile) as f:
        content = f.readlines();
    content = [x.strip() for x in content];
    for line in content:
        splitLine = line.split(",");
        id = int(splitLine[0]);
        if (allPeople[id] != None):
            allPeople[id].cognomen = splitLine[3];

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
            allPeople[id].relativeCertainty.append(True);

#Parse the file containing the grandfather of each person
def parseGrandfatherFile():
    with open(grandfatherFile) as f:
        content = f.readlines();
    content = [x.strip() for x in content];
    for line in content:
        splitLine = line.split(",");
        id = int(splitLine[0]);
        grandfatherID = int(splitLine[1]);
        if (allPeople[id] != None and not(grandfatherID in allPeople[id].relativeIDs)):
            #Allows support for other types of relationships as well
            allPeople[id].relativeIDs.append(grandfatherID);
            allPeople[id].relativeType.append("grandfather");
            allPeople[id].relativeCertainty.append(True);

#Parse the file containing the uncertain fathers
def parseUCFatherFile():
    with open(fatherUCFile) as f:
        content = f.readlines();
    content = [x.strip() for x in content];
    for line in content:
        splitLine = line.split(",");
        id = int(splitLine[0]);
        fatherID = int(splitLine[1]);
        if (allPeople[id] != None):
            #Allows support for other types of relationships as well
            allPeople[id].relativeCertainty[allPeople[id].relativeIDs.index(fatherID)] = False;

#Parse the file containing the grandfather of each person
def parseUCGrandfatherFile():
    with open(grandfatherUCFile) as f:
        content = f.readlines();
    content = [x.strip() for x in content];
    for line in content:
        splitLine = line.split(",");
        id = int(splitLine[0]);
        grandfatherID = int(splitLine[1]);
        if (allPeople[id] != None):
            #Allows support for other types of relationships as well
            allPeople[id].relativeCertainty[allPeople[id].relativeIDs.index(grandfatherID)] = False;

#Parse all relevant files to set up the people needed
def parseInput():
    parseNameIDFile();
    parseNomenFile();
    parseCognomenFile();
    parseBirthFile();
    parseTriumphFile();
    parseFatherFile();
    parseGrandfatherFile();
    parseUCFatherFile();
    parseUCGrandfatherFile();
    
#Sets up the list of relatives to allow easier access for recursion
def buildNodes():
    for person in allPeople:
        if person != None:
            count = 0;
            while (len(person.relativeIDs) > count):
                id = person.relativeIDs[count];
                if (allPeople[id] == None):
                    del person.relativeIDs[count];
                    del person.relativeType[count];
                    del person.relativeCertainty[count];
                else:
                    count = count + 1;
                    
    return;

#Recusrively build up an output giving the highest positions by a person and all their paternal ancestors
#
#Input: conservative - a boolean, true if the output should be limited to directly attested fathers, false if it should guess if none is given
#Input: verbose - a boolean, true if the output should give details like ancestor names
#Input: unity - a boolean, true if the output should be in a format for use in Unity
#input: index - the id of the person to return info on
#Output: A string containing the highest position of this person and their ancestors, formatted nicely
def recursiveRoles(index):
    if allPeople[index] != None:
        person = allPeople[index];
        #Add grandson role if attested
        if "grandfather" in person.relativeType and allPeople[person.relativeIDs[person.relativeType.index("grandfather")]] != None:
            if not(index in allPeople[person.relativeIDs[person.relativeType.index("grandfather")]].relativeIDs):
                allPeople[person.relativeIDs[person.relativeType.index("grandfather")]].relativeIDs.append(index);
                allPeople[person.relativeIDs[person.relativeType.index("grandfather")]].relativeType.append("grandson");
                allPeople[person.relativeIDs[person.relativeType.index("grandfather")]].relativeCertainty.append(person.relativeCertainty[person.relativeType.index("grandfather")]);
        #If a father is attested, it should be used in all cases
        if "father" in person.relativeType and allPeople[person.relativeIDs[person.relativeType.index("father")]] != None:
            if not(index in allPeople[person.relativeIDs[person.relativeType.index("father")]].relativeIDs):
                allPeople[person.relativeIDs[person.relativeType.index("father")]].relativeIDs.append(index);
                allPeople[person.relativeIDs[person.relativeType.index("father")]].relativeType.append("sonConservative");
                allPeople[person.relativeIDs[person.relativeType.index("father")]].relativeCertainty.append(person.relativeCertainty[person.relativeType.index("father")]);
            return;
        else:
            if (index == 1095 or index == 1032 or index == 1235 or index == 4695):
                return;
            #Search the nomen dictionary.  Find the most likely father candidate (someone 22+ years older in the same Gens, ideally with the same cognomen)
            personNomen = person.nomen;
            if (personNomen != ""):
                nomen = nomenDictionary.get(personNomen);
                sameCognomen = [];
                sameNomen = [];
                for x in nomen.people:
                    #Special cases for people whose data is impossible, as above
                    if x.cognomen == person.cognomen and x.birthDate < person.birthDate - 22 and (index != 1235 or x.id != 1095) and (index != 3264 or x.id != 3957) and (index != 3264 or x.id != 3956) and (index != 4453 or x.id != 5311) and (index != 5297 or x.id != 4116) and (index != 5297 or x.id != 4118) and (index != 3788 or x.id != 3573) and (index != 3349 or x.id != 624) and (index != 1235 or x.id != 1032) and (index != 4026 or x.id != 4093):
                        sameCognomen.append(x);
                    if x.birthDate < person.birthDate - 22 and (index != 1235 or x.id != 1095) and (index != 3264 or x.id != 3957) and (index != 3264 or x.id != 3956) and (index != 4453 or x.id != 5311) and (index != 5297 or x.id != 4116) and (index != 5297 or x.id != 4118) and (index != 3788 or x.id != 3573) and (index != 3349 or x.id != 580) and (index != 3349 or x.id != 624) and (index != 1235 or x.id != 1032) and (index != 4026 or x.id != 4093):
                        sameNomen.append(x);
                bestFitId = 0;
                bestFitBirth = -600;
                for x in sameCognomen:
                    if x.birthDate > bestFitBirth:
                        bestFitBirth = x.birthDate;
                        bestFitId = x.id;
                if (len(sameCognomen) == 0 or bestFitId == 0):
                    bestFitId = 0;
                    bestFitBirth = -600;
                    for x in sameNomen:
                        if x.birthDate > bestFitBirth:
                            bestFitBirth = x.birthDate;
                            bestFitId = x.id;
                    if (len(sameNomen) != 0 or bestFitId != 0):
                        allPeople[bestFitId].relativeIDs.append(index);
                        allPeople[bestFitId].relativeType.append("sonHyperaggressive");
                        allPeople[bestFitId].relativeCertainty.append(False);
                        allPeople[index].relativeIDs.append(bestFitId);
                        allPeople[index].relativeType.append("father");
                        allPeople[index].relativeCertainty.append(False);
                    return;
                allPeople[bestFitId].relativeIDs.append(index);
                allPeople[bestFitId].relativeType.append("sonAggressive");
                allPeople[bestFitId].relativeCertainty.append(False);
                allPeople[index].relativeIDs.append(bestFitId);
                allPeople[index].relativeType.append("father");
                allPeople[index].relativeCertainty.append(False);

#Sorts people by death date
def sortDates():
    for indexOne in range(0, len(allDeaths) - 1):
         for indexTwo in range(indexOne + 1, len(allDeaths)):
             if (allDeaths[indexOne][1] > allDeaths[indexTwo][1]):
                 temp = allDeaths[indexOne];
                 allDeaths[indexOne] = allDeaths[indexTwo];
                 allDeaths[indexTwo] = temp;

#Returns a number of spaces equal to indent, for human readability in output files
def printIndent(indent):
    toReturn = "";
    for i in range(0, indent):
        toReturn = toReturn + " ";
    return toReturn;

#Produce the JSON portion of a d3.js tree for a given person and model, working recursively around the network
def printPerson(person, indent, model):
    toReturn = "";
    toReturn = toReturn + printIndent(indent);
    toReturn = toReturn + "{\n";
    indent = indent + 1;
    toReturn = toReturn + printIndent(indent);
    toReturn = toReturn + "\"name\": \"" + person.name + "\",\n";
    toReturn = toReturn + printIndent(indent);
    toReturn = toReturn + "\"id\": \"" + str(person.id) + "\",\n";
    toReturn = toReturn + printIndent(indent);
    toReturn = toReturn + "\"bestPos\": \"" + person.bestPosition + "\",\n";
    toReturn = toReturn + printIndent(indent);
    toReturn = toReturn + "\"birthDate\": \"" + str(person.birthDate) + "\",\n";
    toReturn = toReturn + printIndent(indent);
    toReturn = toReturn + "\"deathDate\": \"" + str(person.deathDate) + "\",\n";
    toReturn = toReturn + printIndent(indent);
    if "father" in person.relativeType and (model == 0 and allPeople[person.relativeIDs[person.relativeType.index("father")]] != None or
                                            model == 1 and (allPeople[person.relativeIDs[person.relativeType.index("father")]] != None and allPeople[person.relativeIDs[person.relativeType.index("father")]].relativeType[allPeople[person.relativeIDs[person.relativeType.index("father")]].relativeIDs.index(person.id)] != "sonHyperaggressive") or
                                            model == 2 and (allPeople[person.relativeIDs[person.relativeType.index("father")]] != None and allPeople[person.relativeIDs[person.relativeType.index("father")]].relativeType[allPeople[person.relativeIDs[person.relativeType.index("father")]].relativeIDs.index(person.id)] == "sonConservative")):
        fatherID = person.relativeIDs[person.relativeType.index("father")];
        toReturn = toReturn + "\"closestAncestor\": \"" + str(fatherID);
        relation = allPeople[fatherID].relativeType[allPeople[fatherID].relativeIDs.index(person.id)];
        toReturn = toReturn + " (" + relation[3::] + ")\",\n";
    else:
        if "grandfather" in person.relativeType and allPeople[person.relativeIDs[person.relativeType.index("grandfather")]] != None:
            fatherID = person.relativeIDs[person.relativeType.index("grandfather")];
            toReturn = toReturn + "\"closestAncestor\": \"" + str(fatherID);
            relation = allPeople[fatherID].relativeType[allPeople[fatherID].relativeIDs.index(person.id)];
            toReturn = toReturn + " (Conservative)\",\n";
        else:
            toReturn = toReturn + "\"closestAncestor\": \"" + "none" + "\",\n";

    val_map = {'consul': 'Praetexta.png',
           'censor': 'Purpurea.png',
           'praetor': 'Praetexta.png',
           'triumphator': 'Picta.png'};
    
    toReturn = toReturn + printIndent(indent);
    toReturn = toReturn + "\"icon\": \"" + (val_map.get(person.bestPosition, "NoPosition.png")) + "\",\n";
    toReturn = toReturn + printIndent(indent);
    toReturn = toReturn + "\"children\": [\n";
    indent = indent + 1;
    hasChildren = False;
    for relIndex in range(0, len(person.relativeType)):
        if "son" in person.relativeType[relIndex] and not("grandson" in person.relativeType[relIndex]) and (model == 0 or model == 1 and person.relativeType[relIndex] != "sonHyperaggressive" or model == 2 and person.relativeType[relIndex] == "sonConservative") or ("grandson" in person.relativeType[relIndex] and not "father" in allPeople[person.relativeIDs[relIndex]].relativeType):
            toReturn = toReturn + printPerson(allPeople[person.relativeIDs[relIndex]], indent, model);
            toReturn = toReturn + ",\n";
            hasChildren = True;
    if hasChildren:
        toReturn = toReturn[0:len(toReturn)-2];
    indent = indent - 1;
    toReturn = toReturn + printIndent(indent);
    toReturn = toReturn + "]\n";
    indent = indent - 1;
    toReturn = toReturn + printIndent(indent);
    toReturn = toReturn + "}";
    return toReturn;

def getHeight(person, model):
    hasChildren = False;
    childDepth = [];
    for relIndex in range(0, len(person.relativeType)):
        if "son" in person.relativeType[relIndex] and not("grandson" in person.relativeType[relIndex]) and (model == 0 or model == 1 and person.relativeType[relIndex] != "sonHyperaggressive" or model == 2 and person.relativeType[relIndex] == "sonConservative") or ("grandson" in person.relativeType[relIndex] and not "father" in allPeople[person.relativeIDs[relIndex]].relativeType):
            childDepth.append(getHeight(allPeople[person.relativeIDs[relIndex]], model));
            hasChildren = True;
    if hasChildren:
        return max(childDepth) + 1;
    return 1;

#Prints the lineage history for all people, sorted by death date
#
#Input: conservative - a boolean, true if the output should be limited to directly attested fathers, false if it should guess if none is given
#Input: verbose - a boolean, true if the output should give details like ancestor names
#Input: unity - a boolean, true if the output should be in a format for use in Unity
def traverseNodes():
    sortDates();
    c = 0;
    d = 0;
    lastDeathRange = -600;
    for death in allDeaths:
        person = allPeople[death[0]];
        recursiveRoles(person.id);

    for death in allDeaths:
        person = allPeople[death[0]];
        if not("father" in person.relativeType) or allPeople[person.relativeIDs[person.relativeType.index("father")]] != None and allPeople[person.relativeIDs[person.relativeType.index("father")]].relativeType[allPeople[person.relativeIDs[person.relativeType.index("father")]].relativeIDs.index(person.id)] != "sonConservative" and not("grandfather" in person.relativeType):
            toPrint = "";
            indent = 0;
            toPrint = toPrint + printPerson(person, indent, 2);
            toPrint = toPrint + ";";
            fp = open('TreeVertical.html', 'r');
            writeFile = 'FamilyTrees/Conservative/' + person.name[0:8] + ' - Conservative.html';
            fw = open(writeFile, 'w');
            line = fp.readline();
            while line:
                fw.write(line);
                if line.find("height =") != -1:
                    fw.write(" " + str(120 * getHeight(person, 2)) + ";");
                if line.find("var treeData =") != -1 and line.find("treemap") == -1:
                    fw.write(toPrint);
                if line.find(".style(\"font-size\", \"16px\")") != -1:
                    fw.write("        .text(\"Descendants of " + person.name + " - Conservative Model\");");
                line = fp.readline();
            fp.close();
            fw.close();

    for death in allDeaths:
        person = allPeople[death[0]];
        while ("father" in person.relativeType and allPeople[person.relativeIDs[person.relativeType.index("father")]] != None and
                                                          person.relativeCertainty[person.relativeType.index("father")] == False and
                                                          "grandfather" in person.relativeType and
                                                          allPeople[person.relativeIDs[person.relativeType.index("grandfather")]] != None and
                                                                    person.relativeCertainty[person.relativeType.index("grandfather")] == True):
            deleteIndex = person.relativeType.index("father");
            fatherID = person.relativeIDs[deleteIndex];
            fatherDeleteID = -1;
            if person.id in allPeople[fatherID].relativeIDs:
                fatherDeleteID = allPeople[fatherID].relativeIDs.index(person.id);
            del person.relativeType[deleteIndex];
            del person.relativeCertainty[deleteIndex];
            del person.relativeIDs[deleteIndex];
            if fatherDeleteID != -1:
                del (allPeople[fatherID].relativeType)[fatherDeleteID];
                del (allPeople[fatherID].relativeCertainty)[fatherDeleteID];
                del (allPeople[fatherID].relativeIDs)[fatherDeleteID];
                if person.id in allPeople[fatherID].relativeIDs:
                    allPeople[fatherID].relativeType[allPeople[fatherID].relativeIDs.index(person.id)] = "grandson";
    
    for death in allDeaths:
        person = allPeople[death[0]];
        if not("father" in person.relativeType) and not("grandfather" in person.relativeType):
            toPrint = "";
            indent = 0;
            toPrint = toPrint + printPerson(person, indent, 0);
            toPrint = toPrint + ";";
            fp = open('TreeVertical.html', 'r');
            writeFile = 'FamilyTrees/Hyperaggressive/' + person.name[0:8] + ' - Hyperaggressive.html';
            fw = open(writeFile, 'w');
            line = fp.readline();
            while line:
                fw.write(line);
                if line.find("height =") != -1:
                    fw.write(" " + str(120 * getHeight(person, 0)) + ";");
                if line.find("var treeData =") != -1 and line.find("treemap") == -1:
                    fw.write(toPrint);
                if line.find(".style(\"font-size\", \"16px\")") != -1:
                    fw.write("        .text(\"Descendants of " + person.name + " - Hyperaggressive Model\");");
                line = fp.readline();
            fp.close();
            fw.close();

    for death in allDeaths:
        person = allPeople[death[0]];
        if (not("father" in person.relativeType) or allPeople[person.relativeIDs[person.relativeType.index("father")]] != None and allPeople[person.relativeIDs[person.relativeType.index("father")]].relativeType[allPeople[person.relativeIDs[person.relativeType.index("father")]].relativeIDs.index(person.id)] == "sonHyperaggressive") and not("grandfather" in person.relativeType):
            toPrint = "";
            indent = 0;
            toPrint = toPrint + printPerson(person, indent, 1);
            toPrint = toPrint + ";";
            fp = open('TreeVertical.html', 'r');
            writeFile = 'FamilyTrees/Aggressive/' + person.name[0:8] + ' - Aggressive.html';
            fw = open(writeFile, 'w');
            line = fp.readline();
            while line:
                fw.write(line);
                if line.find("height =") != -1:
                    fw.write(" " + str(110 * getHeight(person, 1)) + ";");
                if line.find("var treeData =") != -1 and line.find("treemap") == -1:
                    fw.write(toPrint);
                if line.find(".style(\"font-size\", \"16px\")") != -1:
                    fw.write("        .text(\"Descendants of " + person.name + " - Aggressive Model\");");
                line = fp.readline();
            fp.close();
            fw.close();

#Build the people objects, then run the program
parseInput();
buildNodes();
        
traverseNodes();
