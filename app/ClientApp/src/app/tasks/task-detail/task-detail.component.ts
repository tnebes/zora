import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Task } from '../models/task';
import { TaskService } from '../task.service';

@Component({
  selector: 'app-task-detail',
  templateUrl: './task-detail.component.html',
  styleUrls: ['./task-detail.component.css']
})
export class TaskDetailComponent implements OnInit {
  task: Task | null = null;
  taskId: number = 0;
  loading: boolean = true;
  error: string = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private taskService: TaskService
  ) { }

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const idParam = params.get('id');
      if (idParam) {
        this.taskId = +idParam;
        this.loadTask();
      }
    });
  }

  loadTask(): void {
    this.loading = true;
    this.taskService.getTask(this.taskId).subscribe({
      next: (task) => {
        this.task = task;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Error loading task. Please try again later.';
        this.loading = false;
      }
    });
  }

  editTask(): void {
    this.router.navigate(['/tasks/edit', this.taskId]);
  }

  deleteTask(): void {
    if (confirm('Are you sure you want to delete this task?')) {
      this.taskService.deleteTask(this.taskId).subscribe({
        next: () => {
          this.router.navigate(['/tasks']);
        },
        error: (err) => {
          this.error = 'Error deleting task. Please try again later.';
        }
      });
    }
  }

  goBack(): void {
    this.router.navigate(['/tasks']);
  }
}
