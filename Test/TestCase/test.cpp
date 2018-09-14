//template <typename T>
//struct foo {};
//
//template <typename T, typename V>
//struct map {};
//
//template <>
//struct map<int, foo<double>>
//{};
//
//using int32_type = int;
//using float_type = float;
//using my_map = map<int32_type, foo<float_type>>;

/*template <typename T, int a>
struct foo<int, T, a> {};

template <typename Q, Q a>
struct foo<Q, int, a> {}; */

//namespace std
//{
//	template <typename T>
//	struct alloc {};
//	
//	template <typename T, typename Q>
//	struct vector {};
//}
//
//template <typename T>
//using TVector = std::vector<T, std::alloc<T>>;
//
//struct bar
//{
//	using type = TVector<int>;
//};

template <typename T, typename Q>
struct foo {};

template <typename T>
struct foo<double, T> {};

template <>
struct foo<int, double>
{
	foo<double, int> f;
};


//template <typename T>
//using foo_a = foo<T, 2>;
//
//using foo_t = foo_a<int>;