pipeline {
    agent any

    environment {
        DOTNET_ROOT = "/usr/bin/dotnet"
        ASPNETCORE_ENVIRONMENT = "Production"
    }

    stages {
        stage('Setup') {
            parallel {
                stage('Checkout Code') {
                    steps {
                        git branch: 'master', url: 'https://github.com/tnebes/zora.git'
                    }
                }

                stage('Stop Services') {
                    steps {
                        sh 'systemctl stop zora.service'
                        sh 'systemctl stop nginx'
                    }
                }
            }
        }

        stage('Restore Backend Dependencies') {
            steps {
                sh 'cd app && dotnet restore'
            }
        }

        stage('Build and Publish') {
            steps {
                sh 'cd app && dotnet publish -c Release -o ../publish'
            }
        }

        stage('Deploy') {
            steps {
                sh 'rm -rf /var/www/zora/app/*'
                sh 'cp -r publish/. /var/www/zora/app/'
                sh 'sudo systemctl restart zora.service'
                sh 'sudo systemctl restart nginx'
            }
        }
    }
}
