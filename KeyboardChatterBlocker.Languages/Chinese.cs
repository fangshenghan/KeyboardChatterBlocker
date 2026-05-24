namespace KeyboardChatterBlocker.Languages;

public class Chinese : Language
{
	public Chinese()
	{
		language = "简体中文";
		chatter_threshold_label = "按键间隔 (毫秒)";
		ignored_async_keys = "忽略按键(异步)";
		ignored_keys = "忽略按键";
		change_keys = "改变按键";
		listening_keys = "等待按键...";
		clear_keys = "清空按键";
		enable_key_limiter = "开启按键限制器";
		allowed_async_keys = "允许的按键 (异步)";
		allowed_keys = "允许的按键";
		create_key_limiter_profile = "新建配置";
		delete_key_limiter_profile = "删除配置";
		key_limiter_profile_name = "名称";
	}
}
