import gulp from "gulp";
import sass from 'gulp-sass';
import sourcemaps from 'gulp-sourcemaps';
import rename from 'gulp-rename';
import concat from 'gulp-concat';
import autoprefixer from 'gulp-autoprefixer';
import bulkSass from 'gulp-sass-bulk-import';
import gulpif from 'gulp-if';
import fs from 'fs';
import path from 'path';

import config from '../config';


export default class Queue {
    constructor(autorun = true, queue = []) {
        this.running = false;
        this.autorun = autorun;
        this.queue = queue;
        this.previousValue = undefined;
    }

    add(cb) {
        this.queue.push(() => {
            const finished = new Promise((resolve, reject) => {
                const callbackResponse = cb();
                if (callbackResponse !== false) {
                    resolve(callbackResponse);
                } else {
                    reject(callbackResponse);
                }
            });

            finished.then(this.dequeue.bind(this), (() => {}));
        });

        if (this.autorun && !this.running) {
            this.dequeue();
        }

        return this;
    }

    dequeue() {
        this.running = this.queue.shift();

        if (this.running) {
            this.running();
        }

        return this.running;
    }

    get next() {
        return this.dequeue;
    }
}