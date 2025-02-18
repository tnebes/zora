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

        stage('Restore Backend Dependencies') {
            steps {
                sh 'cd app && dotnet restore'
            }
        }

        stage('Build and Publish Backend') {
            steps {
                sh 'cd app && dotnet publish -c Release -o ../publish'
            }
        }

        stage('Deploy') {
            steps {
                sh 'rm -rf /var/www/zora/*'
                sh 'cp -r publish/. /var/www/zora/'
                sh 'sudo systemctl restart zora.service'
            }
        }
    }
}
