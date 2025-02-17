pipeline {
    agent any

    environment {
        DOTNET_ROOT = "/usr/bin/dotnet"
    }

    stages {
        stage('Checkout Code') {
            steps {
                git branch: 'main', url: 'https://github.com/tnebes/zora.git'
            }
        }

        stage('Restore Dependencies') {
            steps {
                sh 'dotnet restore'
            }
        }

        stage('Build') {
            steps {
                sh 'dotnet build --configuration Release'
            }
        }

        stage('Publish') {
            steps {
                sh 'dotnet publish -c Release -o ./publish'
            }
        }

        stage('Deploy') {
            steps {
                sh 'scp -r ./publish/* user@your-server:/var/www/your-app'
                sh 'ssh user@your-server "systemctl restart your-app.service"'
            }
        }
    }
}