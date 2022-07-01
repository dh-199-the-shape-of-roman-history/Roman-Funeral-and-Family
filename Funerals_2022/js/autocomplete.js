/**
 * Contains the logic for filter autocomplete and their clear buttons.
 *
 * Author: Benjamin Niedzielski (benjamin_niedzielski@alumni.brown.edu)
 * Last Modified: June 27, 2022
 */

/**
 * Sets up actions to trigger on person and gens name autocomplete.
 *
 * @param {object} personJSON All possibilities for person autocomplete.
 * @param {object} gensJSON All possibilities for gens autocomplete.
 */
function setupAutocomplete(personJSON, gensJSON) {
    const renderItemFunction = function (ul, item) {
        // In autocomplete options, highlight all text matches.
        let newText = String(item.label).replace(
            new RegExp(this.term, "gi"),
            "<span class=\'ui-state-highlight\'>$&</span>"
        );

        return jQuery("<li></li>")
            .data("item.autocomplete", item)
            .append("<div>" + newText + "</div>")
            .appendTo(ul);
    };

    let $personElem = jQuery("#searchPerson").autocomplete({
        minLength: 3,
        source: personJSON,
        autoFocus: true,
        select: function(event, ui) {
            jQuery("#searchedID").html(ui.item.id);
            jQuery("#searchPerson").val(ui.item.value);
            jQuery("#searchedGensID").html('');
            jQuery("#searchGens").val('');
            jQuery("image.highlighted").each(function(index) {
                jQuery(this).removeClass('highlighted');
                jQuery(this).attr('href', function() {
                    let oldHREF = jQuery(this).attr('href');
                    return oldHREF.substring(0, oldHREF.length - 16) + '.png';
                });
                return true;
            });
            jQuery("g, g *").removeClass('notFocused');
            jQuery("g.funeralGroupInner:not([peopleIncluded*='_" + ui.item.id + "_'])").addClass('notFocused');
            jQuery("image[personID='" + ui.item.id + "']").each(function(index) {
                jQuery(this).addClass('highlighted');
                jQuery(this).attr('href', function() {
                    let oldHREF = jQuery(this).attr('href');
                    if (!oldHREF.includes('highlighted')) {
                        return oldHREF.substring(0, oldHREF.length - 4) + '_highlighted.png';
                    }
                    return oldHREF;
                });
                return true;
            });
            return true;
        }
    }),
    // Different versions of JQuery use different classes.  Ease updating by allowing all types.
    personElemAutocomplete = $personElem.data("ui-autocomplete") || $personElem.data("autocomplete");
    if (personElemAutocomplete) {
        personElemAutocomplete._renderItem = renderItemFunction;
    }

    let $gensElem = jQuery("#searchGens").autocomplete({
        minLength: 3,
        source: gensJSON,
        autoFocus: true,
        select: function(event, ui) {
            jQuery("#searchedGensID").html(ui.item.id);
            jQuery("#searchGens").val(ui.item.value);
            jQuery("#searchedID").html('');
            jQuery("#searchPerson").val('');
            jQuery("image.highlighted").each(function(index) {
                jQuery(this).removeClass('highlighted');
                jQuery(this).attr('href', function() {
                    let oldHREF = jQuery(this).attr('href');
                    return oldHREF.substring(0, oldHREF.length - 16) + '.png';
                });
                return true;
            });
            jQuery("g, g *").removeClass('notFocused');
            jQuery("image:not([gens='" + ui.item.id + "'])").addClass('notFocused');
            return true;
        }
    }),
    // Different versions of JQuery use different classes.  Ease updating by allowing all types.
    gensElemAutocomplete = $gensElem.data("ui-autocomplete") || $gensElem.data("autocomplete");
    if (gensElemAutocomplete) {
        gensElemAutocomplete._renderItem = renderItemFunction;
    }
}

/**
 * Adds autocomplete functionality and reset button functionality
 * on page load.
 */
jQuery(document).ready(function($) {
    setupAutocomplete(autocompleteJSON, gensJSON);

    $('#clearPersonSearch').on('click', function() {
        $("#searchPerson").val('');
        // Make sure clicking clear does not affect other filters.
        if ($("#searchedID").html() != "") {
            $("#searchedID").html('')
            $('g, g *').removeClass('notFocused');
            $("image.highlighted").each(function(index) {
                $(this).removeClass('highlighted');
                $(this).attr('href', function() {
                    let oldHREF = $(this).attr('href');
                    return oldHREF.substring(0, oldHREF.length - 16) + '.png';
                });
                return true;
            });
        }
    });

    $('#clearGensSearch').on('click', function() {
        $("#searchGens").val('');
        // Make sure clicking clear does not affect other filters.
        if ($("#searchedGensID").html() != "") {
            $("#searchedGensID").html('')
            $('g, g *').removeClass('notFocused');
        }
    })
});