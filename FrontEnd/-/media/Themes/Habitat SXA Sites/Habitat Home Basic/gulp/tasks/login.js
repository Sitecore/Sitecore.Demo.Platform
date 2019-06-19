import config from '../config';
import inquirer from 'inquirer';
import gulp from 'gulp';

gulp.task('login', function() {
    var loginPromise = new Promise((resolve, reject) => {
        //if already entered credentials
        if (config.user.login.length) {
            return resolve(config.user);
        }
        inquirer.prompt(config.loginQuestions).then(answer => {
            if (!(answer.login.length && answer.password.length)) {
                reject('loginError');
                process.exit();
            } else {
                resolve(answer)
                config.user = answer
            }
        });
    });
    return loginPromise;
})