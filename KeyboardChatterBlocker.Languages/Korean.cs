namespace KeyboardChatterBlocker.Languages;

public class Korean : Language
{
	public Korean()
	{
		language = "한국어";
		chatter_threshold_label = "채터 임계값 (ms)";
		ignored_async_keys = "무시된 키(Async)";
		ignored_keys = "무시된 키";
		listening_keys = "청취 키...";
		change_keys = "키 변경";
		clear_keys = "모든 키 지우기";
		enable_key_limiter = "키 제한 켜기";
		allowed_async_keys = "입력 허용 키 (비동기)";
		allowed_keys = "입력 허용 키";
		create_key_limiter_profile = "프로필 생성";
		delete_key_limiter_profile = "프로필 삭제";
		key_limiter_profile_name = "이름";
	}
}
