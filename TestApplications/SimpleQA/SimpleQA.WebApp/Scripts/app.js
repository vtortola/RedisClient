$(function () {

    simpleqa.global.errorhandling($);

    var sceneKey = $('body').data('scene');

    if (sceneKey) {
        var scene = simpleqa.scene[sceneKey];

        if (scene)
            scene($);
        else
            throw new Error('Scene key ' + sceneKey + ' not found.');
    }
    else
        console.log('No scene key found.');
});