/*
 * ATTENTION: The "eval" devtool has been used (maybe by default in mode: "development").
 * This devtool is neither made for production nor for readable output files.
 * It uses "eval()" calls to create a separate source file in the browser devtools.
 * If you are trying to read the output file, select a different devtool (https://webpack.js.org/configuration/devtool/)
 * or disable the default devtool with "devtool: false".
 * If you are looking for production-ready output files, see mode: "production" (https://webpack.js.org/configuration/mode/).
 */
(function webpackUniversalModuleDefinition(root, factory) {
	if(typeof exports === 'object' && typeof module === 'object')
		module.exports = factory();
	else if(typeof define === 'function' && define.amd)
		define([], factory);
	else {
		var a = factory();
		for(var i in a) (typeof exports === 'object' ? exports : root)[i] = a[i];
	}
})(self, () => {
return /******/ (() => { // webpackBootstrap
/******/ 	var __webpack_modules__ = ({

/***/ "./src/libs/parsleyjs/i18n/nl.extra.js":
/*!*********************************************!*\
  !*** ./src/libs/parsleyjs/i18n/nl.extra.js ***!
  \*********************************************/
/***/ (() => {

eval("// Validation errors messages for Parsley\n// Load this after Parsley\n\nParsley.addMessages('nl', {\n  dateiso: \"Deze waarde moet een datum in het volgende formaat zijn: (YYYY-MM-DD).\",\n  minwords: \"Deze waarde moet minstens %s woorden bevatten.\",\n  maxwords: \"Deze waarde mag maximaal %s woorden bevatten.\",\n  words: \"Deze waarde moet tussen de %s en %s woorden bevatten.\",\n  gt: \"Deze waarde moet groter dan %s zijn.\",\n  lt: \"Deze waarde moet kleiner dan %s zijn.\"\n});\n\n//# sourceURL=webpack://Vuexy/./src/libs/parsleyjs/i18n/nl.extra.js?");

/***/ })

/******/ 	});
/************************************************************************/
/******/ 	
/******/ 	// startup
/******/ 	// Load entry module and return exports
/******/ 	// This entry module can't be inlined because the eval devtool is used.
/******/ 	var __webpack_exports__ = {};
/******/ 	__webpack_modules__["./src/libs/parsleyjs/i18n/nl.extra.js"]();
/******/ 	
/******/ 	return __webpack_exports__;
/******/ })()
;
});