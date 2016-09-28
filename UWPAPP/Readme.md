This is the UWP app of the project.

You can load the project via `.sln` solution file.

#### Requirements

- Visual Studio
- Universal Windows Platform development SDK
- Windows phone(optional, you can use simulators)

#### Files

- `App.xaml.cs` is the main entrypoint which is auto-generated

- `project.json` stores some Nuget requirements

- `/Assets` stores the logos and icon font

- `/Pages` stores the UI and the main code of the App

- `/Utils` stores some wrapper classes, especially for HTTP get/post

- `/Styles` stores some UI style template

- `/ViewModel` is for data structure for view model, as the architecture is MVVM(Model-View-Viewmodel)

- `/Items` is for data structure for model

- `/Common` is for reserving global data, storing the state

  ​