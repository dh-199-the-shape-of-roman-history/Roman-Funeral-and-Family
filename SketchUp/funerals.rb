

require 'sketchup.rb'

require 'extensions.rb'


module Funerals


unless file_loaded?(__FILE__)

    ex = SketchupExtension.new('Funerals', 'funerals/main')
    ex.description = 'Reads a data file to produce funeral images.'

    ex.version     = '1.0.0'

    ex.creator     = 'Benjamin Niedzielski'

    Sketchup.register_extension(ex, true)

    file_loaded(__FILE__)

end


end # module funerals