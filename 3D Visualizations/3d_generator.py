############################################################################
#This program accepts various CSV files containing information about Romans
#and uses them to generate 3D graphs of their interconnectivity.
#
#Author: Benjamin Niedzielski (bniedzie@ucla.edu)
#Last Modified: 8/20/2019
############################################################################
import networkx as nx;
import matplotlib.pyplot as plt;
import sys;

#Globals
SIZE = 6000;                                        #The array size
allPeople = [None] * SIZE;
allDeaths = [];                                     #Used to sort by death date
personFile = "PersonQuery.csv";                     #Contains rows of Romans, with format of ID, Gender, Name, Nomen, Birth, Death
relationshipFile = "RelationshipQuery.csv";         #Contains rows of relationships, with format of Label, Person 1, Person 2, Relationship Type
pplAdded = set();
linksAdded = set();
nominaAdded = {};

#Each person has relevant information, such as name, birthdate, and relatives
class Person():
    def __init__(self):
        self.name = "";
        self.nomen = "";
        self.gender = "";
        self.id = 0;
        self.birthDate = 0;
        self.deathDate = 0;
        self.relativeIDs = [];
        self.relativeType = [];
        self.relativeCertainty = [];
        self.relatives = [];
        self.bestPosition = "";
        self.gensID = -1;
        self.marked = False;

#Parse the file containing names, ids, and and dates
#creates instances in person class from all people in the personfile
def parsePersonFile():
    with open(personFile) as f:
        content = f.readlines(); #list of every line in csv, e.g. '2, http://romanrepublic.ac.uk/rdf/entity/Sex/Male, TARQ0002 L. Tarquinius (8) Egeri f. Collatinus, Tarquinius, -550, -450\n'
    content = [x.strip() for x in content]; #removes chars (i.e. '\n') from front and end of each line
    for line in content:
        splitLine = line.split(","); #split each line/ entry at the commas and create a list from the resulting (separated) strings, e.g. ['2', 'http://romanrepublic.ac.uk/rdf/entity/Sex/Male', 'TARQ0002 L. Tarquinius (8) Egeri f. Collatinus', 'Tarquinius', '-550', '-450']
        person = Person();
        person.id = int(splitLine[0]);
        if "Female" in splitLine[1]:
            person.gender = "F";
        else:
            person.gender = "M";
        person.name = splitLine[2];
        person.nomen = splitLine[3];
        person.birthDate = int(splitLine[4]);
        person.deathDate = int(splitLine[5]);
        allDeaths.append([person.id, person.deathDate]);
        allPeople[person.id] = person; #instance in person class is placed in nth position in the allPeople array, e.g. person with id no. 2 is placed in index 2 of the array

#Parse the file containing all relationships
def parseRelationshipFile():
    with open(relationshipFile) as f:
        content = f.readlines(); 
    content = [x.strip() for x in content]; 
    for line in content:
        splitLine = line.split(","); 
        #splitline[1] = Person 1/ first person in a relationship/ connection
        splitLine[1] = splitLine[1][splitLine[1].find("Person/")+7:]; #.find("nn") returns index position of the first occurrence of substring "nn"; splitline[1] becomes just the id of Person 1 (i.e. the number behind "Person/")
        #splitline[2] = Person 2 in a relationship/ connection
        splitLine[2] = splitLine[2][splitLine[2].find("Person/")+7:]; #splitline[2] becomes just the id of Person 2
        #splitline[3] = relationship type between Person 1 and Person 2
        splitLine[3] = splitLine[3][splitLine[3].find("Relationship/")+13:]; #splitline[3] becomes just the number representing relationship type (i.e. number behind "Relationship/")
        person1 = int(splitLine[1]); #person1 = id of Person 1
        person2 = int(splitLine[2]);
        relatType = int(splitLine[3]);
        allPeople[person1].relativeIDs.append(person2); #appends Person 2's id number to the relativeIDs object in Person 1's instance in the allPeople array
        allPeople[person1].relativeType.append(relatType); #appends relationship type number between Person 1 and Person 2 to the relativeType object in Person 1's instance in the allPeople array

#Parse all relevant files to set up the people needed
def parseInput():
    parsePersonFile();
    parseRelationshipFile();
    
#Sets up the list of relatives to allow easier access for recursion
def buildNodes():
    for person in allPeople:
        if person != None:
            for id in person.relativeIDs:
                person.relatives.append(allPeople[id]); #appends the instance of every relative of person into the relatives object of the allPeople array
    return;

#Prints <i>indent</i> spaces.  Intended to result in human-readable code and JSON files
def printIndent(indent):
    toReturn = "";
    for i in range(0, indent):
        toReturn = toReturn + " ";
    return toReturn;

#Given a person, determine which group they belong to based on their nomen and return the
#node entry for the JSON file, including ID, name, birth, death.
#Then, recursively call this function on relatives not already visited.
#<i>Model</i> determines which relative types to consider, with
#0 meaning father, mother, and grandfather, and 1 adding in
#son and daughter.
def printPerson(person, indent, model):
    #Mark that we have added this person so we don't duplicate them elsewhere (because it is a set)
    pplAdded.add(person.id);
    person.marked = True;

    #The nomen removes the -a or -us to compare without caring about gender
    if (person.nomen[-1] == "a"): #if last char in nomen is 'a'
        nomen = person.nomen[:-1]; #nomen = nomen without last character
    else:
        nomen = person.nomen[:-2]; #nomen = nomen without last 2 characters

    #Get the group ID by nomen or create a new one
    if nomen in nominaAdded:
        group = nominaAdded[nomen];
    else:
        nominaAdded[nomen] = len(nominaAdded); #nominaAdded is a dict, so key = nomen and value = length of nominaAdded (e.g. {'Valeri': 0})
        group = nominaAdded[nomen]; #group number = length of nominaAdded

    #Prepare the person's node entry
    toReturn = "";
    toReturn = toReturn + printIndent(indent);
    toReturn = toReturn + "{ id: " + str(person.id) + ", group: " + str(group) + ", name: \"" + person.name + "\", birth: " + str(person.birthDate) + ", death: " + str(person.deathDate) + "},\n";

    #For each relative, check if the model calls for examining them.  If so, recusively call this on them
    for i in range(0, len(person.relativeType)):
        if ((person.relativeType[i] == 5 or person.relativeType[i] == 8 or person.relativeType[i] == 19) or #son of, daughter of, grandson of (to find ancestors)
                                                       model > 0 and (person.relativeType[i] == 3 or person.relativeType[i] == 12 or person.relativeType[i] == 25) or model > 1 and (person.relativeType[i] == 4) or model > 2 and (person.relativeType[i] == 7 or person.relativeType[i] == 22 or person.relativeType[i] == 36 or person.relativeType[i] == 41)): #father of, mother of (find posterity)
            #Capture the fact that there is a link between these two people.  Use (min, max) as the ordering to prevent duplicates (b/c the 2 ids have different values, one will be smaller and the other will be larger, so the first value in tuple will be the smaller value, the second will be the larger value)
            #linksAdded is a set so will reject exact duplicates
            linksAdded.add((min(person.id, person.relativeIDs[i]), max(person.id, person.relativeIDs[i]))); #adds tuple to linksAdded set with id of person and id of the person they are related to
            #Visit only people we haven't already added to ensure the recursion terminates (b/c if a person is in pplAdded, then all of the connections to that person have already been added to linksAdded, e.g. if person w/ id 2 is linked to persons w/ ids 3, 4, 5, then when parsing through person 5's connections, there is no need to add the connection btwn person 5 and person 2 again)
            if (person.relativeIDs[i] not in pplAdded):
                toReturn = toReturn + printPerson(person.relatives[i], indent, model); #person.relatives[i] is the whole instance of person in the i position of the relativesID object for the original person, e.g. person.relatives[4] is the instance (of the Person class) of the person in index position 4 of the relativesID list for the original person

    return toReturn;

#Prints the set of edges in the graph
def printLinks(indent):
    toReturn = "";
    for link in linksAdded:
        toReturn = toReturn + printIndent(indent);
        toReturn = toReturn + "{ target: " + str(link[0]) + ", source: " + str(link[1]) + "},\n";
    #We end with an unnecessary "}," so we remove it
    toReturn = toReturn[0:len(toReturn)-2];
    return toReturn;

#Given a person and a type of traversal, create a 3d graph for them based off of the template.
def traversePerson(id, mode):
    #Clear the variables to store information about this person
    pplAdded.clear();
    linksAdded.clear();
    nominaAdded.clear();
    
    toPrint = "";
    indent = 0;
    person = allPeople[id];
    fp = open('3D_Template.html', 'r');

    if (mode == 0):
        writeFile = '3D/' + person.name[0:8] + ' - Simple.html';
        
    if (mode == 1):
        writeFile = '3D/' + person.name[0:8] + ' - Simple with Children.html';
        
    if (mode == 2):
        writeFile = '3D/' + person.name[0:8] + ' - Simple with Children and Spouse.html';
        
    if (mode == 3):
        writeFile = '3D/' + person.name[0:8] + ' - Simple with Children, Spouse and Adopted.html';
    
    fw = open(writeFile, 'w');
    line = fp.readline();
    #Copy the template.  At certain points, add information relevant to this particular person
    while line:
        fw.write(line);
        if line.find("nodes: [") != -1:
            indent = 1;
            fw.write(printPerson(person, indent, mode)[:-2]); #[:-2] removes the unnecessary ",\n" at the end of the list of dicts
        if line.find("links: [") != -1:
            indent = 1;
            fw.write(printLinks(indent));
        line = fp.readline();
    fw.close();
    fp.close();

    return;

#Python's default stack size is too low for this sort of large-scale recursion
sys.setrecursionlimit(3000);

#Build the people objects, then run the program
parseInput();
buildNodes();

def traverseAll(model):
    if (model != 0):
        for person in allPeople:
            if person != None:
                if person.marked == False:
                    traversePerson(person.id, model);

model = input("Please enter the model number for the graph you would like to create (0: simple graph, 1: graph with children, 2: graph with children and spouse, 3: graph with children, spouse and adopted children):\n");
model = int(model);
if (model != 0 and model != 1 and model != 2 and model != 3):
    model = input("Sorry, the number you entered was not a valid model number. Please enter either 0, 1, 2 or 3 as a model number:\n");
    
personID = input("If you would like to create a graph for a specific person, please enter their ID number. Otherwise, if you would like to create graphs for all people, please enter a letter key:\n");

if (personID.isalpha()):
    traverseAll(model);
else:
    person = allPeople[int(personID)];
    if (person not in allPeople or person == None):
        personID = input("Sorry, the number you entered was not a valid ID number. Please enter an ID number between 1 and 6000:\n");
    else:
        traversePerson(int(personID), model);
