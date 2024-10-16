#r "nuget: Fun.Build"

open System.IO
open Fun.Build

let (</>) x y = Path.Combine(x, y)

let appCssPath = "Blazor.Diagram.Demo" </> "wwwroot" </> "app.css"


pipeline "dev" {
    stage "set env" {
        run "npm install"
        run "dotnet restore"
    }
    stage "run services" {
        paralle
        stage "server" {
            workingDir "Blazor.Diagram.Demo"
            run "dotnet watch run"
        }
        run $"npx tailwindcss -i tailwind.css -o {appCssPath} --content \".\**\*.{{html,js,cshtml,razor}}\" --watch"
    }
    runIfOnlySpecified false
}


tryPrintPipelineCommandHelp()
