# Infonetica – Software Engineer Intern Take-Home Exercise 

A minimal backend service providing a RESTful API to define, instantiate, execute, and inspect configurable state machine workflows.

## How to Run:-

1.  **Prerequisites**: .NET 8 SDK
2.  **Cloning**: `git clone https://github.com/abhirupgiri/Infonetica-State-Machine-Backend-System.git`
3.  **Running the Application**:
    Navigate to the project directory by opening the folder in your code editor. Then, run the application using the terminal within the editor:

    ```bash
    dotnet run
4. **Main Progran Files**: The C# files are 1.Program.cs 2.Storage/WorkFlowStore.cs 3.Models/Models.cs

## API Endpoints:-

A summary of the available API endpoints.

* `POST /workflows`: Creates a new workflow definition(This is basically the template of the State Machine).
* `GET /workflows/{id}`: Retrieves a specific workflow definition(This is furthur use of such workflow).
* `POST /workflows/{definitionId}/instances`: Starts a new instance of a workflow. Each Instance is an individual state machine. 
* `GET /instances/{instanceId}`: Retrieves a specific workflow instance.
* `POST /instances/{instanceId}/execute`: Executes an action on a workflow instance. An action can be termed as a transition.

## Pointers:-

* **Persistence**: I used in-memory data storage. I used the 2 dictionaries _definition and _instances to store the templates and the individual state machines respectively.
* **ERROR HANDLING**: I have enforced several validation rules to ensure the correct storage and retrieval of data. During configuration, the system rejects any new workflow definition that does not contain exactly one initial state or is submitted with an ID that already exists. At runtime, the API prevents the execution of an action if the workflow instance is already in a final state, if the requested action does not exist within the definition, or if the instance's current state is not a valid source for that action. It also rejects fetching any workflow or instance which is not avialable in the database.
* **TESTING**: The API was tested end-to-end by creating a workflow definition, starting an instance, executing a sequence of valid actions to move it to a final state, and confirming that validation rules correctly blocked invalid actions on that final state. A command-line tool, curl, was used to send HTTP requests to the API, allowing for end-to-end testing of creating definitions, instantiating workflows, and executing state-transitioning actions.
