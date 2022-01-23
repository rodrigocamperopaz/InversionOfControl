public class MinimalContainer
{
	public delegate object Creator(MinimalContainer container);

	private readonly Dictionary<string, object> configuration
				   = new Dictionary<string, object>();
	private readonly Dictionary<Type, Creator> typeToCreator
				   = new Dictionary<Type, Creator>();

	public Dictionary<string, object> Configuration
	{
		get { return configuration; }
	}

	public void Register<T>(Creator creator)
	{
		typeToCreator.Add(typeof(T), creator);
	}

	public T Create<T>()
	{
		return (T)typeToCreator[typeof(T)](this);
	}

	public T GetConfiguration<T>(string name)
	{
		return (T)configuration[name];
	}
}

class Program
{
	static void Main(string[] args)
	{
		MinimalContainer container = new MinimalContainer();
		//registering dependecies
		container.Register<IRepository>(delegate
		{
			return new NHibernateRepository();
		});
		container.Configuration["email.sender.port"] = 1234;
		container.Register<IEmailSender>(delegate
		{
			return new SmtpEmailSender(container.GetConfiguration<int>("email.sender.port"));
		});
		container.Register<LoginController>(delegate
		{
			return new LoginController(
				container.Create<IRepository>(),
				container.Create<IEmailSender>());
		});

		//using the container
		Console.WriteLine(
			container.Create<LoginController>().EmailSender.Port
		);

		Console.ReadKey();
	}
}

internal class LoginController
{
	private readonly IRepository repository;
	private readonly IEmailSender emailSender;
	
	public LoginController(IRepository repository, IEmailSender emailSender)
    {
		this.repository = repository;
		this.emailSender = emailSender;
    }

	public IRepository Repository { get { return repository; } }

	public IEmailSender EmailSender { get { return emailSender; } }
}

internal interface IRepository
{
}

internal class NHibernateRepository : IRepository
{
    public NHibernateRepository()
    {
    }
}

internal interface IEmailSender
{
	int Port { get; }
}

internal class SmtpEmailSender : IEmailSender
{
	private int port;

	public SmtpEmailSender(int port)
	{
		this.port = port;
	}

	public int Port { get { return port; } }
}