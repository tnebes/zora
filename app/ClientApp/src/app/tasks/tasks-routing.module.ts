import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { TasksComponent } from './tasks/tasks.component';
import { TaskListComponent } from './task-list/task-list.component';
import { TaskDetailComponent } from './task-detail/task-detail.component';
import { TaskEditComponent } from './task-edit/task-edit.component';
import { AuthGuard } from '../core/guards/auth.guard';

const routes: Routes = [
  {
    path: '',
    component: TasksComponent,
    canActivate: [AuthGuard],
    children: [
      { path: '', redirectTo: 'list', pathMatch: 'full' },
      { path: 'list', component: TaskListComponent },
      { path: 'detail/:id', component: TaskDetailComponent },
      { path: 'create', component: TaskEditComponent },
      { path: 'edit/:id', component: TaskEditComponent }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class TasksRoutingModule { }
