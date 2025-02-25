import {enableProdMode} from '@angular/core';
import {platformBrowserDynamic} from '@angular/platform-browser-dynamic';

import {AppModule} from './app/app.module';
import {environment} from './environments/environment';
import {Constants} from './app/core/constants';

export function getBaseUrl() {
    return document.getElementsByTagName('base')[0].href;
}

const providers = [
    {provide: 'BASE_URL', useFactory: getBaseUrl, deps: []}
];

function createDevelopmentBanner() {
    const banner = document.createElement('div');
    banner.style.position = 'fixed';
    banner.style.top = '0';
    banner.style.left = '0';
    banner.style.width = '100%';
    banner.style.backgroundColor = '#2196F3';
    banner.style.color = 'white';
    banner.style.textAlign = 'center';
    banner.style.padding = '5px';
    banner.style.zIndex = '1000';
    banner.style.fontWeight = 'bold';
    banner.innerText = 'DEVELOPMENT ENVIRONMENT';
    document.body.prepend(banner);
}

function createCopyrightFooter() {
    const footer = document.createElement('footer');
    footer.style.position = 'fixed';
    footer.style.bottom = '0';
    footer.style.left = '0';
    footer.style.width = '100%';
    footer.style.backgroundColor = '#333';
    footer.style.color = 'white';
    footer.style.textAlign = 'center';
    footer.style.padding = '5px';
    footer.style.zIndex = '1000';
    footer.innerText = `© ${new Date().getFullYear()} ${Constants.COMPANY_NAME}`;
    document.body.appendChild(footer);
}

if (environment.production) {
    enableProdMode();
    createCopyrightFooter();
} else {
    console.log('Development mode');
    createDevelopmentBanner();
}

platformBrowserDynamic(providers).bootstrapModule(AppModule)
    .catch(err => {
        console.error('An error occurred during application bootstrap:', err);
    });
