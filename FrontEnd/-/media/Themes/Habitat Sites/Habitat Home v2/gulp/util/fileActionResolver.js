import config from '../config';
import upload from 'gulp-upload';
import path from 'path';
import gulp from 'gulp';
import urllib from 'urllib';
import formstream from 'formstream';
import request from 'request';
import formData from 'form-data';
import * as fs from 'fs';
import querystring from 'querystring';
import deleteFile from './requestDeleteFile';
import changeFile from './requestChangeFile';
import skipSending from './skipSending';
import changeTemplate from './requestChangeTemplate';
import Queue from './Queue';

const queue = new Queue();
export default function(file) {
    let fileName = path.basename(file.path),
        eventType = file.event; // add, unlink , change 
    if (skipSending(file.path)) {
        console.log(('Sending of file ' + fileName + ' was skipped').blue);
        return;
    }
    if (fileName.indexOf('.html') > -1) {
        return queue.add(function() {
            return changeTemplate(file)
        });
    }
    if (eventType == 'change' || eventType == 'add') {
        queue.add(function() {
            return changeFile(file)
        });

    } else if (eventType == 'unlink') {
        queue.add(function() {
            return deleteFile(file);
        });

    }

};