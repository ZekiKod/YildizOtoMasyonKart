import{dxDropDownTagName as t}from"./dropdown-91641129.js";import{a as o,b as e,n as s}from"./popup-e76e963a.js";import{_ as r}from"./tslib.es6-d65164b3.js";import{s as i,i as l,x as a}from"./lit-element-462e7ad3.js";import{n as p}from"./property-4ec0b52d.js";import{e as n}from"./custom-element-267f9a21.js";import{dxBranchTagName as d}from"./branch-aebd078a.js";import{D as m}from"./popupportal-e116a31b.js";import{d as h}from"./events-interseptor-a522582a.js";const b="dxbl-dropdown-root";let u=class extends i{constructor(){super(...arguments),this.topLeftClass=null,this.topRightClass=null,this.bottomLeftClass=null,this.bottomRightClass=null,this.dropOpposite=!1,this.dropDirection=o.Near,this.dropAlignment=e.bottom,this.resizing=!1}static get styles(){return l`
            :host {
                display: flex;
                position: relative;
                flex: 1 1 auto;
                flex-direction: column;
                box-sizing: border-box;
                min-height: 0;
            }
            .hidden {
                display: none;
            }
            ::slotted([slot="top-right"]) {
                position: absolute;
                z-index: 1001;
                top: 0px;
                right: 0px;
                transform: rotate(270deg);
                cursor: ne-resize;
            }
            ::slotted([slot="top-left"]) {
                position: absolute;
                z-index: 1001;
                top: 0px;
                left: 0px;
                transform: rotate(180deg);
                cursor: nw-resize;
            }
            ::slotted([slot="bottom-left"]) {
                position: absolute;
                z-index: 1001;
                bottom: 0px;
                left: 0px;
                transform: rotate(90deg);
                cursor: sw-resize;
            }
            ::slotted([slot="bottom-right"]) {
                position: absolute;
                z-index: 1001;
                bottom: 0px;
                right: 0px;
                cursor: se-resize;
                transform: rotate(0deg);
            }
        }`}connectedCallback(){super.connectedCallback(),this.calculateStyles(this.resizing,this.dropAlignment,this.dropDirection)}willUpdate(t){(t.has("dropAlignment")||t.has("dropDirection")||t.has("resizing"))&&this.calculateStyles(this.resizing,this.dropAlignment,this.dropDirection)}calculateStyles(t,s,r){this.topLeftClass=t&&s===e.top&&r===o.Far?null:"hidden",this.topRightClass=t&&s===e.top&&r===o.Near?null:"hidden",this.bottomLeftClass=t&&s===e.bottom&&r===o.Far?null:"hidden",this.bottomRightClass=t&&s===e.bottom&&r===o.Near?null:"hidden"}render(){return a`
            <slot></slot>
            <dxbl-thumb data-qa-thumb-location="top-left" data-dropdown-thumb class="${this.topLeftClass}">
                <slot name="top-left"></slot>
            </dxbl-thumb>
            <dxbl-thumb data-qa-thumb-location="top-right" data-dropdown-thumb class="${this.topRightClass}">
                <slot name="top-right"></slot>
            </dxbl-thumb>
            <dxbl-thumb data-qa-thumb-location="bottom-left" data-dropdown-thumb class="${this.bottomLeftClass}">
                <slot name="bottom-left"></slot>
            </dxbl-thumb>
            <dxbl-thumb data-qa-thumb-location="bottom-right" data-dropdown-thumb class="${this.bottomRightClass}">
                <slot name="bottom-right"></slot>
            </dxbl-thumb>`}};r([p({type:String,reflect:!1})],u.prototype,"topLeftClass",void 0),r([p({type:String,reflect:!1})],u.prototype,"topRightClass",void 0),r([p({type:String,reflect:!1})],u.prototype,"bottomLeftClass",void 0),r([p({type:String,reflect:!1})],u.prototype,"bottomRightClass",void 0),r([p({type:Object,attribute:"drop-opposite"})],u.prototype,"dropOpposite",void 0),r([p({type:String,attribute:"drop-direction"})],u.prototype,"dropDirection",void 0),r([p({type:String,attribute:"drop-alignment"})],u.prototype,"dropAlignment",void 0),r([p({type:Boolean,attribute:"resizing"})],u.prototype,"resizing",void 0),u=r([n(b)],u);const c=[t,s,b,d,m,h];function g(t){if(!t)throw new Error("failed");return t}const f=Object.freeze({__proto__:null,registeredComponents:c,getReference:g,default:{getReference:g,registeredComponents:c}});export{u as D,f as d,c as r};
