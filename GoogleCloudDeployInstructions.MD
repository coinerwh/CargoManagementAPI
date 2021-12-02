Google Cloud set up:

1. Add app.yaml file to root of project folder
2. Add Item Group to include app.yaml in output directory <br />
   <ItemGroup
   <None Include="app.yaml" CopyToOutputDirectory="Always" 
   </ItemGroup

3. Set project: gcloud config set project {project_name}
4. Install Google.Cloud.Datastore.V1 latest version

Local Dev:

Add the following to the launchsettings.json under environmentvariables:

"GOOGLE_PROJECT_ID": "project_id_from_google", <br />
"GOOGLE_APPLICATION_CREDENTIALS": "/location/of/service/credentials" <br />

Steps to Deploy to Google Cloud:

1. Create app.yaml file in root folder
    - include the following in the file: <br />
    runtime: aspnetcore <br />
    env: flex

2. Run "dotnet publish -c Release" to create the compiled app
3. Run "gcloud projects list" then "gcloud config set project `PROJECT ID`" to select project
4. Run "gcloud app deploy ./bin/Release/netcoreapp3.1/publish/app.yaml --version v0"
    - This will create the image and deploy to the project VM in Google Cloud

5. Run "gcloud app browse" to launch the app in your browser" 

