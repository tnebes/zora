import { Component } from '@angular/core';

@Component({
  selector: 'app-control-panel',
  template: `
    <div class="container-fluid">
      <div class="row">
        <div class="col-md-3">
          <app-sidebar></app-sidebar>
        </div>
        <div class="col-md-9">
          <router-outlet></router-outlet>
        </div>
      </div>
    </div>
  `
})
export class ControlPanelComponent { }