import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { Task, TaskResponseDto } from '../../tasks/models/task';
import { TaskService } from '../../core/services/task.service';

@Component({
    selector: 'app-priority-tasks',
    templateUrl: './priority-tasks.component.html',
    styleUrls: ['./priority-tasks.component.css']
})
export class PriorityTasksComponent implements OnInit {
    public tasks$: Observable<Task[]>;

    constructor(
        private readonly taskService: TaskService,
        private readonly router: Router
    ) {
        this.tasks$ = this.taskService.searchTasks({
            page: 1,
            pageSize: 5,
            priority: 'High',
            sortColumn: 'duedate',
            sortDirection: 'asc'
        }).pipe(
            map(response => response.items)
        );
    }

    ngOnInit(): void { }

    public navigateToTask(taskId: number): void {
        this.router.navigate(['/tasks/detail', taskId]);
    }
} 