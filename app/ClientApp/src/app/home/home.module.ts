import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { HomeComponent } from './home.component';
import { PriorityTasksComponent } from './priority-tasks/priority-tasks.component';

import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';

@NgModule({
    declarations: [
        HomeComponent,
        PriorityTasksComponent
    ],
    imports: [
        CommonModule,
        RouterModule,
        MatCardModule,
        MatIconModule,
        MatProgressBarModule
    ],
    exports: [HomeComponent]
})
export class HomeModule { } 