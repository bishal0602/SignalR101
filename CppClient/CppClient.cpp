#include <iostream>
#include "hub_connection_builder.h"
#include <future>

using namespace std;
int main()
{
	promise<void> start_task;
	signalr::hub_connection connection = signalr::hub_connection_builder::create("https://localhost:7056/messagehub")
		//.skip_negotiation(true)
		.build();


	connection.start([&start_task](exception_ptr exc) {
		try
		{
			start_task.set_value();
			if (exc != nullptr)
			{
				rethrow_exception(exc);
			}
		}
		catch (const exception& ex)
		{
			cout << "Connection couldnt be established: " << ex.what() << endl;
			exit(1);
		}
		});
	start_task.get_future().get();

	connection.on("ReceiveMessage", [](const vector<signalr::value>& message)
		{
			cout << endl << message[0].as_string() << endl;
		});

	while (connection.get_connection_state() == signalr::connection_state::connected)
	{
		string message;
		getline(cin, message);

		if (message == ":q" || connection.get_connection_state() != signalr::connection_state::connected)
		{
			break;
		}

		vector<signalr::value> arguments{message};
		connection.invoke("BroadcastMessage", arguments, [](const signalr::value& value, exception_ptr exc) {
			try
			{
				if (exc != nullptr)
				{
					rethrow_exception(exc);
				}
			}
			catch (const exception& ex)
			{
				cout << "Message couldnt be broadcasted: " << ex.what() << endl;
			}
			});

		promise<void> stopTask;
		connection.stop([&stopTask](exception_ptr exc)
			{
				try
				{
					if (exc)
					{
						rethrow_exception(exc);
					}

					cout << "Conenction closed succesfully" << endl;
				}
				catch (const exception& e)
				{
					cout << "Unable to close connection: " << e.what() << endl;
				}

				stopTask.set_value();
			});
		stopTask.get_future().get();
	}

	return 0;
}
