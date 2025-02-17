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

        stage('Build Frontend') {
            steps {
                sh 'cd app/ClientApp && npm run build -- --configuration=production'
            }
        }

        stage('Restore Backend Dependencies') {
            steps {
                sh 'cd app && dotnet restore'
            }
        }

        stage('Build Backend') {
            steps {
                sh 'cd app && dotnet build --configuration Release'
            }
        }

        stage('Publish Backend') {
            steps {
                sh 'cd app && dotnet publish -c Release -o ../publish'
            }
        }

        stage('Deploy') {
            steps {
                sh 'rm -rf /var/www/zora/*'
                sh 'rm -rf /var/www/zora/wwwroot/*'
                sh 'cp -r publish/. /var/www/zora/'
                sh 'cp -r app/ClientApp/dist/. /var/www/zora/wwwroot/'
                sh 'sudo systemctl restart zora.service'
            }
        }
    }
}
