import extend from 'extend';
import path from 'path';
import configUtils from './util/setThemePath';

export default {

    // Change to TRUE if you want add source map for sass files
    sassSourceMap: false,
    // Please configure
    serverOptions: {
        server: 'https://habitathome.dev.local', //need to be changed
        removeScriptPath: '/-/script/v2/master/RemoveMedia',
        uploadScriptPath: '/sitecore modules/PowerShell/Services/RemoteScriptCall.ashx',
        updateTemplatePath: '/-/script/v2/master/ChangeTemplate',
        mediLibraryPath: '/-/script/media/master'
    },

    autoprefixer: {
        browsers: ['last 2 versions',
            '> 1%',
            'ie 9',
            'opera 12.1',
            'ios 6',
            'android 4'
        ],
        cascade: false
    },
    //Rules for excluding files from uploading
    excludedPath: [
        'styles/has.css', // can be a string
        ///[\\\/][\w-]*.css$/g   //exclude all css files
        /[\\\/]test.css$/g, //exclude test.css files
        /[\\\/][\w-]*.css.map$/g //exclude css.map files
        ///styles[\\\/][\w-]*.css$/g //exclude all css files from style folder
    ],
    //Server check all items names with this rule. It is not recommended to change
    serverNameValidation: [
        /^[\w\*\$][\w\s\-\$]*(\(\d{1,}\)){0,1}$/
    ],
    minifyOptions: {
        js: {
            compress: {
                hoist_funs: true,
                passes: 1
            },
            toplevel: false
        },
        css: { compatibility: 'ie8' }
    },

    html: {
        path: (function () {
            let rootCreativeExchangePath = global.rootPath.split('-\\media'),
                _path = './';
            if (rootCreativeExchangePath.length > 1) {
                _path = path.relative('./', global.rootPath.split('-\\media')[0])
            }
            return _path + '/**/*.html';
        })()
    },
    img: {
        path: 'images/**/*'
    },
    js: {
        path: 'scripts/**/*.js',
        esLintUploadOnError: true,
        minificationPath: ['scripts/**/*.js'],
        jsOptimiserFilePath: 'scripts/**/',
        jsOptimiserFileName: 'pre-optimized-min.js',
        es6Support: false,
        jsSourceMap: false,
        enableMinification: false,
        disableSourceUploading: false
    },
    es: {
        path: 'sources/**/*.js',
        targetPath: 'scripts/',
        disableSourceUploading: false
    },
    css: {
        path: 'styles/**/*.css',
        targetPath: '',
        minificationPath: ['styles/*.css'],
        cssOptimiserFilePath: 'styles/',
        cssOptimiserFileName: 'pre-optimized-min.css',
        cssSourceMap: false,
        enableMinification: false,
        disableSourceUploading: false
    },
    sass: {
        root: 'sass/**/*.scss',
        components: {
            sassPath: 'sass/*.scss',
            stylePath: 'styles'
        },
        styles: {
            sassPath: ['sass/styles/common/*.scss',
                'sass/styles/content-alignment/*.scss',
                'sass/styles/layout/*.scss'
            ],
            stylePath: 'styles',
            concatName: 'styles.css'
        },
        dependency: {
            sassPath: ['sass/styles/**/*.scss'],
            exclusion: ['!sass/styles/common/*.scss',
                '!sass/styles/content-alignment/*.scss',
                '!sass/styles/layout/*.scss'
            ],
        },
        core: {
            sassPath: ['sass/abstracts/**/*.scss',
                'sass/base/**/*.scss',
                'sass/components/**/*.scss'
            ],
            stylePath: 'styles'
        },
        disableSourceUploading: false
    },

    sprites: {
        flags: {
            spritesmith: {
                imgName: 'sprite-flag.png',
                cssName: '_sprite-flag.scss',
                imgPath: '../images/sprite-flag',
                cssFormat: 'scss',
                padding: 10,
                algorithm: 'top-down',
                cssOpts: {
                    cssSelector: function (sprite) {
                        return '.flags-' + sprite.name;
                    }
                },
                cssVarMap: function (sprite) {
                    sprite.name = 'flags-' + sprite.name;
                }

            },
            flagsFolder: 'images/flags/*.png',
            imgDest: './images',
            cssDest: './sass/base/sprites'
        }

    },

    stylesConfig: {
        'accordion': 'component-accordion.scss',
        'breadcrumb': 'component-breadcrumb.scss',
        'carousel': 'component-carousel.scss',
        'container': 'component-container.scss',
        'divider': 'component-divider.scss',
        'feed': 'component-feed.scss',
        'flip': 'component-flip.scss',
        'forms': 'component-forms.scss',
        'image': 'component-image.scss',
        'link-list': 'component-link-list.scss',
        'navigation': 'component-navigation.scss',
        'play-list': 'component-playlist.scss',
        'promo': 'component-promo.scss',
        'rich-text': 'component-richtext-content.scss',
        'tabs': 'component-tabs.scss',
        'file-list': 'component-file-list.scss',
        'media-link': 'component-media-link.scss',
        'search': 'component-search-other.scss',
        'galleria': 'component-galleria.scss',
        'archive': 'component-archive.scss',
        'field-editor': 'component-field-editor.scss',
        'map': 'component-map.scss',
        'page-content': 'component-richtext-content.scss',
        'page-list': 'component-page-list.scss',
        'tag-cloud': 'component-tag-cloud.scss',
        'tag-list': 'component-tag-list.scss',
        'title': 'component-title.scss',
    },

    loginQuestions: [{
        type: 'login',
        name: 'login',
        message: 'Enter your login',
        default: 'sitecore\\admin'
    },
    {
        type: 'password',
        name: 'password',
        message: 'Enter your password',
        default: 'b'
    }
    ],


    user: { login: '', password: '' },

    init: function () {
        extend(this.serverOptions, configUtils.getConf().serverOptions);
        return this;
    }

}.init();