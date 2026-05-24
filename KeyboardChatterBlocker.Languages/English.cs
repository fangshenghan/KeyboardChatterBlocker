namespace KeyboardChatterBlocker.Languages;

public class English : Language
{
	public English()
	{
		language = "English";
		chatter_threshold_label = "Chatter Threshold (ms)";
		ignored_async_keys = "Ignored Keys(Async)";
		ignored_keys = "Ignored Keys";
		listening_keys = "Listening Keys...";
		change_keys = "Change Keys";
		clear_keys = "Clear Keys";
		enable_key_limiter = "Enable Key Limiter";
		allowed_async_keys = "Allowed Keys (Async)";
		allowed_keys = "Allowed Keys";
		create_key_limiter_profile = "Create Profile";
		delete_key_limiter_profile = "Delete Profile";
		key_limiter_profile_name = "Name";
	}
}
