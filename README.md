# CarRental



FIRST TIME:

git clone ...

dotnet restore

dotnet ef database update



EVERYTIME BEFORE MAKING CHANGES:

\-> git pull origin main (git bash)

\-> dotnet ef database update (inside the slnx project)



EVERYTIME AFTER MAKING CHANGES:

\-> Make a new branch: each developer runs ONCE git checkout -b your name

\-> Run this for any changes in db: dotnet ef migrations add AddSomething

&#x09;			   dotnet ef database update

\-> Run this to commit: git add .

&#x09;	       git commit -m "Add: short clear description"



\-> Final push in you branch: git push origin your name

TO MERGE EACH DEVELOPER'S CHANGE RUN ON GITHUB:

\-> Click the banner on GitHub saying: Compare \& pull request

\-> The banner above merges your branch to main 

