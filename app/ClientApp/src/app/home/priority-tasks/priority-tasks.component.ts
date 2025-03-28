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
    public readonly emptyTaskMessages = [
        "Congratulations! You've achieved the impossible - an empty task list! Time to celebrate with a coffee break! ☕",
        "No tasks? Your productivity is so high, you've broken the system! 🚀",
        "Your task list is as empty as a developer's social life! 😅",
        "Task list empty? You must be living in a parallel universe where deadlines don't exist! 🌌",
        "No tasks found. Your efficiency is off the charts! 📈"
    ];

    constructor(
        private readonly taskService: TaskService,
        private readonly router: Router
    ) {
        this.tasks$ = this.taskService.getPriorityTasks().pipe(
            map(response => response.items)
        );
    }

    ngOnInit(): void { }

    public navigateToTask(taskId: number): void {
        this.router.navigate(['/tasks/detail', taskId]);
    }

    public getRandomMessage(): string {
        const randomIndex = Math.floor(Math.random() * this.emptyTaskMessages.length);
        return this.emptyTaskMessages[randomIndex];
    }
} 