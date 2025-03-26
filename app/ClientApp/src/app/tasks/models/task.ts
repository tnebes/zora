export interface Task {
    id: number;
    title: string;
    description: string;
    priority: string;
    status: string;
    dueDate: Date;
    completionPercentage: number;
    assigneeId?: number;
    name: string;
    startDate?: string;
    estimatedHours?: number;
    actualHours?: number;
    createdAt: string;
    createdById: number;
    updatedAt?: string | null;
    updatedById?: number | null;
    assigneeName?: string;
    projectId?: number;
    parentTaskId?: number | null;
}

export interface TaskResponseDto {
    items: Task[];
    total: number;
    page: number;
    pageSize: number;
}
