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

template <typename T>
struct foo {};

template <typename T, template <typename Q> class TP>
struct bar {};

//template <>
//struct bar<int, foo>
//{};

//using my_bar = bar<foo<foo<double>>, foo>;
template<>
struct bar<foo<foo<double>>, foo>
{
	
};