var gm = require('gm').subClass({ imageMagick: true });

console.log('Exporting functions from imageFormatter.js');

module.exports = {
    cropAndRotateImage: function (callback, tmpFile, crop, rotate) {
        console.log('tmpFile' + tmpFile);

        const res = gm(tmpFile);

        if(rotate && rotate.degrees) {
            console.log('rotate parameters color -> ' + rotate.color + ', degrees -> ' + rotate.degrees);
            res.rotate(rotate.color || 'black', rotate.degrees);
        }

        if(crop) {
            console.log('crop parameters width -> ' + crop.width + ', height -> ' + crop.height + ', left -> ' + crop.left + ', top -> ' + crop.top);
            res.crop(crop.width, crop.height, crop.left, crop.top);
        }

        res.write(tmpFile, function(err) {
            if (err) {
                callback(new Error('Error while cropping image: ' + err), false);
            }
            return callback(null, true);
        });
    },

    resizeImage: function(result, tmpFile, resize) {
        console.log('tmpFile' + tmpFile);

        console.log('resize parameters folder -> ' + resize.folder + ', width -> ' + resize.width + ', height -> ' + resize.height + ', quality -> ' + resize.quality);

        gm(tmpFile)
            .background('#000000')
            .resize(resize.width, resize.height, '>')
            .gravity('Center')
            .extent(resize.width, resize.height)
            .quality(resize.quality || DEFAULT_QUALITY)
            .stream(function (err, stdout, stderr) {
                if(err) {
                    console.error('Error while resizing', resize, stderr);
                    throw new Error('Error while resizing image: ' + err);
                }
                stdout.pipe(result.stream);
            });
    }
};