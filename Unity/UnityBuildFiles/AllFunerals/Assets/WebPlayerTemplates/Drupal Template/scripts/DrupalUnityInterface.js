var DrupalUnityInterface;

(function() {

  var Interface = function() {

  }

  /*
   * Get the current environment object loaded by Drupal
   *
   * @return {Object}  environment object
   * @return {Integer} environment.id - the enviroment id
   * @return {String}  enviroment.title - the title, or name, of the enviroment
   * @return {Integer} enviroment.current_tour_id - the id of the tour that is currently loaded
   *
   * NOTE: This can be synchronous
   *
   */
  Interface.prototype.getCurrentEnvironment = function(gameObject, method) {
    console.log("get environment "+gameObject+" "+method);
    var enviroment = {
    "id": 6,
    "title": "Test Environment",
    "current_tour_id": 123
	};
    var environment_json = JSON.stringify(enviroment);

    u.getUnity().SendMessage(gameObject, method, environment_json);
  }

  /*
   * Return the tour specified by the given tour_id
   *
   * @param  {Integer} tour_id - Numerical enviroment id from which to retrieve tours
   *
   * @return {Object}  tour - the tour object
   * @return {String}    tour.title - the title, or name, of the tour
   * @return {Integer}   tour.id - the tour id
   * @return {String}    tour.placards - an array of placard objects
   *
   * NOTE: This should be asynchronous, is there a promise implementation we can use,
   * or should we add a callback as an argument?
   *
   */
  Interface.prototype.getTour = function(tour_id, gameObject, method) {
    console.log("get tour "+tour_id+" "+gameObject+" "+method);
    if (tour_id != 123) {
      u.getUnity().SendMessage(gameObject, method, "false");
      return false;
    }
    var tour = {
    "title": "Test tour",
    "id": 123,
    "placards": [
        {
            "id": 3241,
            "title": "First placard in our test tour",
            "latitude": 12.1212,
            "longitude": 60.3455,
            "elevation": 20,
            "orientation": 355
        },
        {
            "id": 3242,
            "title": "Second placard in our test tour",
            "latitude": 12.14,
            "longitude": 60.5,
            "elevation": 20,
            "orientation": 15
        }
    ]
	};

    var tour_json = JSON.stringify(tour);

    u.getUnity().SendMessage(gameObject, method, tour_json);
  }

  /*
   * Return all in-world objects for a given enviroment id
   *
   * In-world objects are identical to placards, but not associated with any particular tour
   * Instead of stopping points along a journey they are unordered, clickable spots in the virtual world
   *
   * @param {Integer} environment_id  - Numerical enviroment id from which to retrieve in-world objects
   * @return {Array} in_world_objects - an array of in world objects, or false if an invalid environment_id is specified
   *
   * NOTE: This should be asynchronous, is there a promise implementation we can use,
   * or should we add a callback as an argument?
   *
   */
  Interface.prototype.getInWorldObjects = function(environment_id, gameObject, method) {
    console.log("get in world objects "+environment_id+" "+gameObject+" "+method);
    if (environment_id != 6) {
      u.getUnity().SendMessage(gameObject, method, "false");
      return false;
    }
    var in_world_objects = {
    "placards": [
        {
            "id": 2344,
            "title": "First in-world object",
            "latitude": 12.1212,
            "longitude": 60.3455,
            "elevation": 20,
            "orientation": 355
        },
        {
            "id": 4466,
            "title": "Second in-world object",
            "latitude": 12.14,
            "longitude": 60.5,
            "elevation": 45,
            "orientation": 15
        }
    ]
};
    var in_world_objects_json = JSON.stringify(in_world_objects);
    u.getUnity().SendMessage(gameObject, method, in_world_objects_json);
  }

  /*
   *  Inform Drupal that the user has clicked an in-world object or placard
   *
   *  @param {Integer} placard_id - the id of the placard / in game object that has been clicked
   *
   *  If provided callback send true on success, false on failure
   */
  Interface.prototype.registerPlacardClick = function(placard_id, gameObject, method) {
    console.log("register placard click "+placard_id+" "+gameObject+" "+method);
    if (typeof(placard_id) != 'number') {
      throw new Error('Placard id sent to registerPlacardClick must be an integer!');
    }
    u.getUnity().SendMessage(gameObject, method, "true");
  }

  /*
   *  Inform Drupal that the user wishes to add placard lat, long, elevation, orientation defined
   *  by provided placard object
   *
   *  @param {Integer} tour_id - the id of the tour to which to add a placard
   *  @param {String}  placard_json - JSON representing the placard to add
   *
   *  Placard i.e.
   *  {
   *    latitude: 45.002
   *    longitude: 13.002
   *    orientation: 359,
   *    elevation: 54,
   *  }
   *
   */
  Interface.prototype.addPlacard = function(tour_id, placard_json, gameObject, method) {
    var placard = JSON.parse(placard_json);
    console.log("add placard "+tour_id+" "+placard+" "+gameObject+" "+method);
    if (typeof(tour_id) !== 'number' || typeof(placard) != 'object') {
      throw new Error('Invalid arguments sent to DrupalUnityInterface.addPlacard. Should be tour_id (integer), placard (object)');
    }
    var required_properties = ['latitude', 'longitude', 'elevation', 'orientation'];
    for (var index in required_properties) {
      var property = required_properties[index];
      if (typeof(placard[property]) == 'undefined') {
        throw new Error('placard.'+ property +' must be set on placard object passed to DrupalUnityInterface.addPlacard');
      }
    }
    u.getUnity().SendMessage(gameObject, method, "true");
  }

  DrupalUnityInterface = new Interface();

})();




