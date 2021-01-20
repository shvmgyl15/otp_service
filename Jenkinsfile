pipeline {

	agent any
	stages {
		
   
		stage(' Build and Publish Docker Image to Azure Container Registry') {
                        steps {
                                withCredentials([string(credentialsId: 'Azure-Container-Registry', variable: 'SECRET')]) {
					sh 'az acr login --name $SECRET'
					sh 'docker build . -t $SECRET".azurecr.io/vaccination-center-test:${BUILD_NUMBER}"'
					sh 'docker push $SECRET".azurecr.io/otp-service-test:${BUILD_NUMBER}"'
				}

                        }
                        post {
                                success {
                                        echo "Published Docker Image to Azure Container Registry"
                                }
                        }
                }
 
                stage('Deploy Application in AKS') {
                        steps {
				sh 'kubectl apply -f kubernetes/deployment.yml'
                                sh 'kubectl apply -f kubernetes/service.yml'
				
				withCredentials([string(credentialsId: 'Azure-Container-Registry', variable: 'SECRET')]) {
                                        sh 'kubectl set image deployment/otp-service-test  otp-service-test=$SECRET".azurecr.io/otp-service-test:${BUILD_NUMBER}" -n consent-manager'
                                }
                        }
                        post {
                                success {
                                        echo "Deployed to AKS"
                                }
                        }
                }

		
	}
}


