pipeline {
    agent any

    environment {
        DOTNET_ROOT = "/usr/bin/dotnet"
        ASPNETCORE_ENVIRONMENT = "Production"
    }

    stages {
        stage('Checkout Code') {
            steps {
                git branch: 'master', url: 'https://github.com/tnebes/zora.git'
            }
        }

        stage('Install Frontend Dependencies') {
            steps {
                sh 'cd app/ClientApp && npm install --legacy-peer-deps'
            }
        }

        stage('Restore Dependencies') {
            steps {
                sh 'cd app && dotnet restore'
            }
        }

        stage('Build') {
            steps {
                sh 'cd app && dotnet build --configuration Release'
            }
        }

        stage('Publish') {
            steps {
                sh 'cd app && dotnet publish -c Release -o ../publish'
            }
        }

        stage('Deploy') {
            steps {
                sh 'cp -r publish/* /var/www/zora/'
                sh 'cp -r app/ClientApp/dist/* /var/www/zora/wwwroot/'
                sh 'sudo systemctl restart zora.service'
            }
        }
    }
}