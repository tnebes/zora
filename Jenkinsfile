pipeline {
    agent any

    environment {
        DOTNET_ROOT = "/usr/bin/dotnet"
        ASPNETCORE_ENVIRONMENT = "Production"
        SOLUTION_DIR = "app"
        PROJECT_DIR = "${SOLUTION_DIR}/src"
        TESTS_DIR = "${SOLUTION_DIR}/tests/zora.Tests"
        PUBLISH_DIR = "publish"
        DEPLOY_DIR = "/var/www/zora"
    }

    stages {
        stage('Checkout') {
            steps {
                checkout scm
            }
        }

        stage('Stop Services') {
            steps {
                sh 'sudo systemctl stop zora.service || true'
                sh 'sudo systemctl stop nginx || true'
            }
        }

        stage('Restore Dependencies') {
            steps {
                sh '''
                    cd ${SOLUTION_DIR}
                    dotnet restore
                '''
            }
        }

        stage('Build') {
            steps {
                sh '''
                    cd ${SOLUTION_DIR}
                    dotnet build --configuration Release --no-restore
                '''
            }
        }

        stage('Publish') {
            steps {
                sh '''
                    cd ${PROJECT_DIR}
                    dotnet publish zora.csproj --configuration Release --no-build -o ../../${PUBLISH_DIR}
                '''
            }
        }

        stage('Deploy') {
            steps {
                sh '''
                    rm -rf ${DEPLOY_DIR}/app/*
                    cp -r ${PUBLISH_DIR}/. ${DEPLOY_DIR}/app/
                    
                    # The frontend should be built by the PublishRunWebpack target in the .csproj
                    # Ensure the wwwroot folder exists in the deploy directory
                    mkdir -p ${DEPLOY_DIR}/wwwroot
                '''
            }
        }

        stage('Start Services') {
            steps {
                sh '''
                    sudo systemctl start zora.service
                    sudo systemctl start nginx
                '''
            }
        }

        stage('Verify Deployment') {
            steps {
                sh '''
                    echo "Verifying zora service status..."
                    systemctl is-active --quiet zora.service && echo "Zora service is running successfully" || (echo "Zora service failed to start" && exit 1)
                    
                    echo "Verifying nginx status..."
                    systemctl is-active --quiet nginx && echo "Nginx is running successfully" || (echo "Nginx failed to start" && exit 1)
                '''
            }
        }
    }

    post {
        // always {
        //     cleanWs()
        // }
        success {
            echo 'Deployment completed successfully!'
        }
        failure {
            echo 'Deployment failed!'
            sh 'sudo systemctl status zora.service || true'
            sh 'sudo systemctl status nginx || true'
            sh 'tail -n 100 /var/log/zora/app.log || true'
        }
    }
}
