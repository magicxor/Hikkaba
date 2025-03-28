'use strict';

const gulp = require('gulp');
const rename = require('gulp-rename');
const $ = require('gulp-load-plugins')({
    pattern: ['gulp-*']
});

const errorHandler = function (title) {
    return function (err) {
        $.util.log($.util.colors.red('[' + title + ']'), err.toString());
        this.emit('end');
    };
};

const outputDir = './_output/css-bundle';

gulp.task('styles', function() {
    return gulp.src('./css/site.css')
        .pipe($.sourcemaps.init())
        .pipe($.cleanCss({ advanced: true }))
        .pipe(rename({ suffix: '.min' }))
        .pipe($.sourcemaps.write('.'))
        .pipe(gulp.dest(outputDir));
});
