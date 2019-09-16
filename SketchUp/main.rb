###########################################################
#Adds 2 Sketchup Plugin commands to interface with input
#data describing Roman funerals.  One generates images
#approximating each funeral, and the other visualizes
#all funerals in 25 year increments.
#
#Author: Benjamin Niedzielski (bniedzie@ucla.edu)
#Last Modified: 9/13/18
###########################################################



require 'sketchup.rb'


require 'pp'

module Funerals



#Creates a funeral on the Rostra in the Forum for each person
#in the input file, generating an image showing the funeral
#for each.
def self.create_funeral
    #The following variables determine input/output files and should be edited accordingly
    #NOTE: The Components should also have their paths changed later in the code
    outputPath = "C:/Users/Benja/Desktop/FuneralsTest/"
    inputPath = 'C:/Users/Benja/Desktop/testfile.txt'

    #Initialize variables used to interface with SketchUp
    model = Sketchup.active_model

    model.start_operation('Create Funeral', true)

    writeFile = ""
    group = nil
    entities = nil
    view = nil

    lineCount = 1

    #Each funeral has 3 lines in the input - the first has name and death year,
    #the second has all positions to show, and the third is blank
    File.foreach(inputPath) {|x|
        if lineCount % 3 == 1
	    #Create a new group
            group = model.active_entities.add_group

            entities = group.entities

            view = Sketchup.active_model.active_view
	
	    #Determine the output filename and the text to display based on this line
	    writeFile = x[0..7]
	    writeFile = outputPath + writeFile + ".jpg"
            point16 = Geom::Point3d.new(-300, 4380, -385)
            text = entities.add_text(x, point16)
        elsif lineCount % 3 == 2
	    #First, set up the deceased icon
	    deceasedPoint = Geom::Point3d.new(-480, 4480, -415)
	    transform = Geom::Transformation.new deceasedPoint
	    definitions = model.definitions

	    positions = x.split()
	    #If the deceased had no major office, the line will start with a space or be empty
	    #Otherwise, the first position is not an ancestor, so remove it after rendering.
	    if positions.length == 0 || x[0] == " " 
		path = Sketchup.find_support_file "BierStandard.skp", "Components/Funeral/"
	    elsif positions[0] == "triumphator"
	    	path = Sketchup.find_support_file "BierPicta.skp", "Components/Funeral/"
		positions.delete_at(0)
	    elsif positions[0] == "consul"
	        path = Sketchup.find_support_file "BierPraetexta.skp", "Components/Funeral/"
		positions.delete_at(0)
	    elsif positions[0] == "censor"
	        path = Sketchup.find_support_file "BierPurpurea.skp", "Components/Funeral/"
		positions.delete_at(0)
	    elsif positions[0] == "praetor"
	        path = Sketchup.find_support_file "BierPraetexta.skp", "Components/Funeral/"
		positions.delete_at(0)
	    else
		print "error"
	    end
	    componentdefinition = definitions.load path
	    instance = entities.add_instance componentdefinition, transform

            #If any ancestors remain
	    if positions.length != 0
		#Initial coordinates set to output on the third step of the Rostra
		#Moving from right to left, so the oldest is on the left
		#The y position changes quadratically to match the curve
		#All units are in inches
	        xVal = -710
	        yVal = 4225
                xDiff = 40
	        yDiff = -8
	        yDiffDiff = 3

	    	positions.each {|pos|
		    #For each ancestor, update the x and y coordinates
		    xVal = xVal + xDiff
		    yVal = yVal + yDiff
		    yDiff = yDiff + yDiffDiff

		    #Create the ancestor component at the right location
		    point = Geom::Point3d.new xVal,yVal,-386
		    transform = Geom::Transformation.new point
		    definitions = model.definitions
	            if pos == "triumphator"
	    	        path = Sketchup.find_support_file "Picta.skp", "Components/Funeral/"
		    elsif pos == "consul"
	    	        path = Sketchup.find_support_file "Praetexta.skp", "Components/Funeral/"
		    elsif pos == "censor"
	    	        path = Sketchup.find_support_file "Purpurea.skp", "Components/Funeral/"
		    elsif pos == "praetor"
	    	        path = Sketchup.find_support_file "Praetexta.skp", "Components/Funeral/"
		    end
		    componentdefinition = definitions.load path
	    	    instance = entities.add_instance componentdefinition, transform
	        }
	    end
            model.commit_operation

	
	    #Generate the image
	    keys = {
  		:filename => writeFile,
  		:width => 640,
  		:height => 480,
  		:antialias => false,
  		:compression => 0.9,
  		:transparent => true
	    }
	    model = Sketchup.active_model
	    view = model.active_view
	    view.write_image keys

	    #Remove the previous group
	    group.erase!
        end
        lineCount = lineCount + 1
    }
end

#Creates a visualization of all funerals in the input file,
#generating one column for every 25 year period
def self.create_visualization
    #The following variable determines the input file location
    #NOTE: Change as well the paths of the components later in the code
    inputFile = 'C:/Users/Benja/Desktop/testfile.txt'  

    #Set up SketchUp interfacing
    model = Sketchup.active_model

    model.start_operation('Create Visualization', true)

    view = Sketchup.active_model.active_view
    group = model.active_entities.add_group

    entities = group.entities


    #Set up variables for positioning funerals correctly
    baseXVal = 440
    baseYVal = 0
    xDiff = -40
    yDiff = 50
    xPos = baseXVal
    yPos = baseYVal

    eraEnd = -500   #funerals before 500 BCE go in the first column
    lineCount = 1

    #Each funeral has 3 lines in the input - the first has name and death year,
    #the second has all positions to show, and the third is blank
    File.foreach(inputFile) {|x|
        if lineCount % 3 == 1
	    #Get the death date to see if we need a new column
	    info = x.split()
            deathDate = info[info.length - 1].to_i
            if deathDate > eraEnd
		#Create a new group for each column
		group = model.active_entities.add_group

    		entities = group.entities


		#Update the date that ends this column
	        eraEnd = deathDate / 25 * 25
		eraEnd = eraEnd + 25
	        #print deathDate.to_s + " " + eraEnd.to_s + "\n"
                
		#Revert to the bottom of the column
		yPos = baseYVal
	        if lineCount != 1
			#Don't move over if we're still on the first funeral
                	baseXVal = baseXVal + 520
		end
                xPos = baseXVal
	    end
        elsif lineCount % 3 == 2
	    positions = x.split()

	    #If the deceased had a position, remove it
	    if positions.length != 0 && x[0] != " " 
		positions.delete_at(0)
	    end

	    #If there was at least 1 ancestor
            if positions.length != 0
		#Ensure all funerals start at the same x position
		#Even though icons will be made right-to-left
		#so the oldest ancestor will be on the left
	        xPos = baseXVal - (12 - positions.length) * 40
	    	
		#Create the right component for each ancestor, moving to the left each time
		positions.each {|pos|
		    point = Geom::Point3d.new xPos,yPos,0
		    transform = Geom::Transformation.new point
		    definitions = model.definitions
	            if pos == "triumphator"
	    	        path = Sketchup.find_support_file "Picta.skp", "Components/Funeral/"
		    elsif pos == "consul"
	    	        path = Sketchup.find_support_file "Praetexta.skp", "Components/Funeral/"
		    elsif pos == "censor"
	    	        path = Sketchup.find_support_file "Purpurea.skp", "Components/Funeral/"
		    elsif pos == "praetor"
	    	        path = Sketchup.find_support_file "Praetexta.skp", "Components/Funeral/"
		    end
		    componentdefinition = definitions.load path
	    	    instance = entities.add_instance componentdefinition, transform
		    xPos = xPos + xDiff
	        }
		#Revert x to the right position for this column and move up a row
	        xPos = baseXVal
	        yPos = yPos + yDiff
	    end
            model.commit_operation

        end
        lineCount = lineCount + 1
    }
end

unless file_loaded?(__FILE__)

    menu = UI.menu('Plugins')

    menu.add_item('Rostra Funerals') {

        self.create_funeral
    }

    menu.add_item('Visualize Funerals') {

        self.create_visualization
    }

    file_loaded(__FILE__)

end


end # module Funerals