pipeline {
    agent any
	steps {
    	stages {
			stage('Run All') {
				parallel {
					stage('Build') {
						agent any
						steps {			
							echo 'Building..'			
											
							script {							
								result = bat (
									script: '@git describe --tags --long --dirty --always',
									returnStdout: true
								).trim()
												
								dir("./src/projects/syncwatchertray/build")
								{
									bat "powershell -ExecutionPolicy Bypass -File ./build.ps1 -buildNumber=${env.BUILD_NUMBER} -gitVersion=${result} -gitBranch=${env.BRANCH_NAME} --buildconfig=Install"
								}
							}
						}
					}

					stage('Inspect') {
						agent any
						steps {			
							echo 'Inspecting..'			
									
							script {													
								dir("./src/shared/build/inspect")
								{
									bat "powershell -ExecutionPolicy Bypass -File inspect.ps1 -solution=../../../Solutions/Projects.sln -output=../../../projects/syncwatchertray/build/publish -failoninspect=false"
								}
							}
						}
					}
				}
					
				stage ('Report') {
					steps {
						echo 'Reporting...'
						dir("./src/projects/syncwatchertray/build")
						{
							echo 'Scanning logs....'
							
							warnings canComputeNew: false, canResolveRelativePaths: false, categoriesPattern: '', consoleParsers: [[parserName: 'MSBuild'], [parserName: 'CodeAnalysis']], parserConfigurations: [[parserName: 'Resharper InspectCode', pattern: '**\\publish\\*.inspect.xml']], defaultEncoding: '', excludePattern: '', healthy: '', includePattern: '', messagesPattern: '', unHealthy: ''
						}	
					}
				}
				stage('Deploy') {
					steps {
						echo 'Deploying...'				
						dir("./src/projects/syncwatchertray/build")
						{
							archiveArtifacts artifacts: 'publish/*', fingerprint: true
						}				
					}
				}
			}
		}			
	}
	
	post {	
        always {
			deleteDir()
		}
	}
}