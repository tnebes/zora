import { Component, OnInit, ViewChild } from '@angular/core';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { Router } from '@angular/router';
import { Task } from '../models/task';
import { TaskService } from '../task.service';

@Component({
  selector: 'app-task-list',
  templateUrl: './task-list.component.html',
  styleUrls: ['./task-list.component.css']
})
export class TaskListComponent implements OnInit {
  displayedColumns: string[] = ['id', 'title', 'status', 'dueDate', 'assignedUserName', 'actions'];
  dataSource: MatTableDataSource<Task> = new MatTableDataSource<Task>([]);
  totalCount: number = 0;
  pageSize: number = 10;
  currentPage: number = 0;

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private taskService: TaskService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.loadTasks();
  }

  loadTasks(): void {
    this.taskService.getTasks(this.currentPage + 1, this.pageSize)
      .subscribe(response => {
        this.dataSource = new MatTableDataSource(response.tasks);
        this.totalCount = response.totalCount;
        
        // Set up sorting after data is loaded
        if (this.sort) {
          this.dataSource.sort = this.sort;
        }
      });
  }

  onPageChange(event: any): void {
    this.currentPage = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadTasks();
  }

  viewTask(taskId: number): void {
    this.router.navigate(['/tasks/detail', taskId]);
  }

  editTask(taskId: number): void {
    this.router.navigate(['/tasks/edit', taskId]);
  }

  createTask(): void {
    this.router.navigate(['/tasks/create']);
  }

  deleteTask(taskId: number): void {
    if (confirm('Are you sure you want to delete this task?')) {
      this.taskService.deleteTask(taskId).subscribe(() => {
        this.loadTasks();
      });
    }
  }
}
